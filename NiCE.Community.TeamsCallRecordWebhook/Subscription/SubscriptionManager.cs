// <copyright file="SubscriptionManager.cs" company="NiCE IT Management Solutions GmbH">
// Copyright (c) NiCE IT Management Solutions GmbH. All rights reserved.
// </copyright>

namespace NiCE.Community.TeamsCallRecordWebhook.Subscription
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using NiCE.A365.Collector.Authentication;
    using NiCE.Community.TeamsCallRecordWebhook.Configuration;
    using NiCE.Community.TeamsCallRecordWebhook.Controllers;
    using NiCE.Community.TeamsCallRecordWebhook.Interfaces;

    /// <summary>
    /// Manages the subscriptions.
    /// </summary>
    public class SubscriptionManager : ISubscriptionManager, IAsyncDisposable
    {
        private const int RenewIntervalSeconds = 60 * 10; // 10 minutes
        private const int SubscriptionExpirationTimeInSeconds = 4230 * 60;

        private readonly ILogger<SubscriptionManager> logger;

        private readonly Serializer serializer = new ();

        private readonly ConcurrentDictionary<Guid, List<SubscriptionCache>> enabledSubscriptions;

        private readonly string clientState = Guid.NewGuid().ToString();

        private readonly Task renewTask;

        private ApiConfiguration options;

        private CancellationTokenSource? cts;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionManager"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger{TCategoryName}"/> added by DI.</param>
        /// <param name="options"><see cref="IOptionsMonitor{TOptions}"/> added by DI.</param>
        public SubscriptionManager(ILogger<SubscriptionManager> logger, IOptionsMonitor<ApiConfiguration> options)
        {
            this.logger = logger;

            this.options = new ApiConfiguration();

            this.options = options.CurrentValue;
            options.OnChange((o) =>
            {
                this.options = o;
            });

            this.enabledSubscriptions = new ConcurrentDictionary<Guid, List<SubscriptionCache>>();

            this.cts = new CancellationTokenSource();
            this.renewTask = Task.Run(async () =>
            {
                var task = this.DoRenewSubscriptions(this.cts.Token);

                try
                {
                    if (!task.IsCompleted)
                    {
                        await task.ConfigureAwait(false);
                    }
                }
                catch
                {
                    if (!task.IsCanceled && task.IsFaulted)
                    {
                        logger.LogError(0, task.Exception, "Error while Renewing Subscriptions.");
                    }
                }
            });
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SubscriptionManager"/> class.
        /// </summary>
        ~SubscriptionManager()
        {
            this.DisposeAsync().GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public event EventHandler<CallRecordArgs>? CallRecordCreated;

        /// <inheritdoc/>
        public event EventHandler<CallRecordArgs>? CallRecordUpdated;

        /// <inheritdoc/>
        public event EventHandler<CallRecordArgs>? CallRecordDeleted;

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await this.DisposeAsyncCore().ConfigureAwait(false);

            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public ActionResult<string> ProcessNotification(HttpRequest request, string bodyData, string? validationToken = null)
        {
            const string oDataTypeKey = "oDataType";
            const string oDataTypeCallRecord = "#microsoft.graph.callrecord";
            const string IdKey = "id";

            if (validationToken is { })
            {
                // validation request.
                this.logger.LogInformation("Returning Token for validation");
                return new OkObjectResult(validationToken);
            }

            this.logger.LogInformation("Process ProcessNotification Data: {Data}", bodyData);

            var content = this.serializer.DeserializeObject<ChangeNotificationCollection>(bodyData);

            if (content.Value is null)
            {
                this.logger.LogInformation("Invalid Value");
                return new BadRequestResult();
            }

            // loop through each changeNotification object to process each separate
            foreach (var changeNotification in content.Value)
            {
                if (!this.clientState.Equals(changeNotification.ClientState))
                {
                    this.logger.LogInformation("Invalid ClientState");

                    // ClientState is wrong
                    continue;
                }

                if (changeNotification.ResourceData is null || changeNotification.TenantId is null)
                {
                    this.logger.LogInformation("Missing ResourceData or TenantId");
                    continue;
                }

                var addData = changeNotification.ResourceData.AdditionalData;

                if (addData is { } && addData.TryGetValue(oDataTypeKey, out var dataType))
                {
                    // call methods to process different type of notifies.
                    switch (dataType.ToString())
                    {
                        case oDataTypeCallRecord:
                            {
                                if (addData.TryGetValue(IdKey, out var obj) && obj.ToString() is string id && Guid.TryParse(id, out var callRecordId))
                                {
                                    this.ProcessCallRecord(changeNotification.TenantId.Value, callRecordId, changeNotification.ChangeType);
                                }
                                else
                                {
                                    this.logger.LogError("Can't get the callRecordId", dataType);
                                }

                                break;
                            }

                        default:
                            {
                                this.logger.LogInformation("Receive Change Notification: {ChangeNotification}", JsonSerializer.Serialize(changeNotification));
                                break;
                            }
                    }
                }
                else
                {
                    // TODO: log message
                }
            }

            return new AcceptedResult();
        }

        /// <inheritdoc/>
        public async Task<bool> RegisterSubscriptionAsync(Guid directoryId, SubscriptionResource resource, string resourcePath, CancellationToken ctx = default)
        {
            // resourceUrl = "communications/callRecords"
            var enabledResources = this.enabledSubscriptions.GetOrAdd(directoryId, (t) => new List<SubscriptionCache>());

            if (enabledResources.Any(c => c.Resource == resource && c.ResourcePath.Equals(resourcePath)))
            {
                return true;
            }

            try
            {
                // get application data.
                var app = AzureApp.GetApp(directoryId).FirstOrDefault();

                if (app is null)
                {
                    throw new KeyNotFoundException($"No application was register for the directory {directoryId}.");
                }

                // Get graph client
                var client = app.GetGraphServiceClient();

                // create subscription object that is used for the registration.
                var subscription = new Subscription()
                {
                    ChangeType = "created",
                    NotificationUrl = new Uri(new Uri(this.options.PublicEndpoint), HttpRouteConstants.SubscriptionController).AbsoluteUri,
                    Resource = resourcePath,
                    ClientState = this.clientState,
                    ExpirationDateTime = DateTimeOffset.UtcNow.AddSeconds(SubscriptionExpirationTimeInSeconds),
                };

                var response = await client.Subscriptions.Request().AddAsync(subscription, ctx).ConfigureAwait(false);

                // add subscription data to cache
                var cache = new SubscriptionCache(
                    Guid.Parse(response.Id),
                    directoryId,
                    resource,
                    response.Resource,
                    response.ExpirationDateTime?.DateTime ?? DateTime.UtcNow.AddSeconds(SubscriptionExpirationTimeInSeconds));

                this.logger.LogInformation("Register subscription for resource {Resource} from Tenant {TenantId}(SubId: {SubId})", cache.Resource, directoryId, response.Id);

                enabledResources.Add(cache);

                return true;
            }
            catch (OperationCanceledException e) when (e.CancellationToken == ctx)
            {
                throw;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while registering an subscription");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveSubscriptionAsync(Guid directoryId, SubscriptionResource resource, CancellationToken ctx = default)
        {
            if (!this.enabledSubscriptions.TryGetValue(directoryId, out var subscriptionCaches))
            {
                this.logger.LogDebug("Can't remove Subscription for resource {Resource} from Tenant {TenantId} the tenant has no known subscriptions. ", resource, directoryId);
                return true;
            }

            // get cached subscription data
            var subscriptionCache = subscriptionCaches.SingleOrDefault(c => c.Resource == resource);

            if (subscriptionCache is null)
            {
                this.logger.LogDebug("Can't remove Subscription for resource {Resource} from Tenant {TenantId} because this resource has no subscription under this tenant. ", resource, directoryId);
                return false;
            }

            this.logger.LogInformation("Remove Subscription for resource {Resource} from Tenant {TenantId}(SubId: {SubId})", resource, directoryId, subscriptionCache.Id);

            try
            {
                // get application data.
                var app = AzureApp.GetApp(directoryId).FirstOrDefault();

                if (app is null)
                {
                    throw new KeyNotFoundException($"No application was register for the directory {directoryId}.");
                }

                // Get graph client and update subscription
                var client = app.GetGraphServiceClient();

                await client.Subscriptions[subscriptionCache.Id.ToString()].Request().DeleteAsync(ctx).ConfigureAwait(false);

                // remove cached subscription data
                subscriptionCaches.Remove(subscriptionCache);

                if (subscriptionCaches.Count == 0)
                {
                    // remove tenant if is no longer contains cached subscription data
                    this.enabledSubscriptions.TryRemove(directoryId, out _);
                }
            }
            catch (OperationCanceledException e) when (e.CancellationToken == ctx)
            {
                throw;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while removing the subscription {SubId}", subscriptionCache.Id);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (this.cts is { })
            {
                this.cts.Cancel();
                await this.renewTask.ConfigureAwait(false);
                this.cts.Dispose();
                this.cts = null;
            }

            if (!this.enabledSubscriptions.IsEmpty)
            {
                this.logger.LogInformation("Remove Subscriptions");
            }

            // remove all current registered subscriptions.
            foreach (var (tenantId, list) in this.enabledSubscriptions.ToArray())
            {
                foreach (var subscriptionCache in list.ToArray())
                {
                    await this.RemoveSubscriptionAsync(tenantId, subscriptionCache.Resource).ConfigureAwait(false);
                }
            }
        }

        private async Task<bool> RenewSubscription(Guid directoryId, Guid subId, CancellationToken ctx = default)
        {
            if (!this.enabledSubscriptions.TryGetValue(directoryId, out var subscriptionCaches))
            {
                this.logger.LogDebug("Can't renew Subscription from Tenant {TenantId} because tenant has no known subscriptions. ", directoryId, subId);
                return false;
            }

            // get cached subscription data
            var subscriptionCache = subscriptionCaches.SingleOrDefault(c => c.Id.Equals(subId));

            if (subscriptionCache is null)
            {
                this.logger.LogDebug("Can't renew Subscription from Tenant {TenantId} because SubId {SubId} don't exist under this tenant. ", directoryId, subId);
                return false;
            }

            this.logger.LogInformation("Renew Subscription for resource {Resource} from Tenant {TenantId}(SubId: {SubId})", subscriptionCache.Resource, directoryId, subId);

            // create object for update
            var subUpdate = new Subscription
            {
                ExpirationDateTime = DateTimeOffset.UtcNow.AddSeconds(SubscriptionExpirationTimeInSeconds),
            };

            try
            {
                // get application data.
                var app = AzureApp.GetApp(directoryId).FirstOrDefault();

                if (app is null)
                {
                    throw new KeyNotFoundException($"No application was register for the directory {directoryId}.");
                }

                // Get graph client and update subscription
                var client = app.GetGraphServiceClient();
                var response = await client.Subscriptions[subscriptionCache.Id.ToString()].Request().UpdateAsync(subUpdate, ctx).ConfigureAwait(false);

                // update subscription data in cache
                subscriptionCache.ExpirationDateTime = response.ExpirationDateTime?.DateTime ?? DateTime.UtcNow.AddHours(1);
            }
            catch (OperationCanceledException e) when (e.CancellationToken == ctx)
            {
                throw;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while removing the subscription {SubId}", subscriptionCache.Id);
                return false;
            }

            return true;
        }

        private async Task DoRenewSubscriptions(CancellationToken ct)
        {
            IEnumerable<Task> Renew(DateTime checkTime)
            {
                foreach (var (tenantId, subscriptions) in this.enabledSubscriptions)
                {
                    foreach (var subscription in subscriptions)
                    {
                        if (subscription.ExpirationDateTime < checkTime)
                        {
                            yield return this.RenewSubscription(tenantId, subscription.Id, ct);
                        }
                    }
                }
            }

            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(RenewIntervalSeconds), ct).ConfigureAwait(false);
                this.logger.LogDebug("Check if Subscriptions for Renew exist");

                var checkTime = DateTime.UtcNow.AddSeconds(RenewIntervalSeconds * 2);

                await Task.WhenAll(Renew(checkTime)).ConfigureAwait(false);

                ct.ThrowIfCancellationRequested();
            }
        }

        private void ProcessCallRecord(Guid tenantId, Guid callRecordId, ChangeType? change)
        {
            // call events for different change types.
            switch (change)
            {
                case ChangeType.Created:
                    {
                        this.logger.LogInformation("Notification New CallRecord TenantId:'{tenantId}', CallRecordId:'{callRecordId}'", tenantId, callRecordId);
                        this.CallRecordCreated?.Invoke(this, new CallRecordArgs(tenantId, callRecordId));
                        break;
                    }

                case ChangeType.Updated:
                    {
                        this.logger.LogInformation("Notification Update CallRecord TenantId:'{tenantId}', CallRecordId:'{callRecordId}'", tenantId, callRecordId);
                        this.CallRecordUpdated?.Invoke(this, new CallRecordArgs(tenantId, callRecordId));
                        break;
                    }

                case ChangeType.Deleted:
                    {
                        this.logger.LogInformation("Notification Deleted CallRecord TenantId:'{tenantId}', CallRecordId:'{callRecordId}'", tenantId, callRecordId);
                        this.CallRecordDeleted?.Invoke(this, new CallRecordArgs(tenantId, callRecordId));
                        break;
                    }

                case null:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
