// <copyright file="SubscriptionController.cs" company="NiCE IT Management Solutions GmbH">
// Copyright (c) NiCE IT Management Solutions GmbH. All rights reserved.
// </copyright>

namespace NiCE.Community.TeamsCallRecordWebhook.Controllers
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.Extensions.Logging;
    using NiCE.A365.Collector.Authentication;
    using NiCE.Community.TeamsCallRecordWebhook.Interfaces;
    using NiCE.Community.TeamsCallRecordWebhook.Subscription;

    /// <summary>
    /// Controller for the Subscription Endpoints.
    /// </summary>
    [ApiController]

    // [Route("api/[controller]")]
    [Route(HttpRouteConstants.SubscriptionController)]
    public class SubscriptionController : ControllerBase
    {
        private readonly ILogger<SubscriptionController> logger;

        private readonly ISubscriptionManager subscriptionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionController"/> class.
        /// </summary>
        /// <param name="logger">Logger to log event.</param>
        /// <param name="subscriptionManager">The subscription Manager manages the subscription.</param>
        public SubscriptionController(ILogger<SubscriptionController> logger, ISubscriptionManager subscriptionManager)
        {
            this.logger = logger;
            this.subscriptionManager = subscriptionManager;
        }

        /*
         * WebHook Endpoint
         *
         *
         */

        /// <summary>
        /// Route to validate the Subscription.
        /// </summary>
        /// <param name="value">Value contains the CallRecord Data.</param>
        /// <param name="validationToken">Token send from Graph Api to confirm the subscription.</param>
        /// <returns>Returns ok to the Graph Api.</returns>
        [HttpPost]
        [Route("")]
        public ActionResult<string> Post([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] object? value = null, [FromQuery] string? validationToken = null)
        {
            return this.subscriptionManager.ProcessNotification(this.Request, value?.ToString() ?? string.Empty, validationToken);
        }

        /// <summary>
        /// Route to register the Webhook.
        /// </summary>
        /// <param name="tenantId">The Id of the Tenant.</param>
        /// <param name="resource">The resource for the webhook.</param>
        /// <param name="resourceUrl">The Url at the Graph Api Endpoint.</param>
        /// <param name="applicationId">The Id of the application to be registered.</param>
        /// <param name="applicationSecret">The application secret.</param>
        /// <param name="ctx">The cancelation token.</param>
        /// <returns>Returns success or failure.</returns>
        [HttpGet]
        [Route("register")]
        public async Task<ActionResult<string>> RegisterGet(
            [FromQuery] Guid tenantId,
            [FromQuery] SubscriptionResource resource,
            [FromQuery] string resourceUrl,
            [FromQuery] Guid? applicationId,
            [FromQuery] string? applicationSecret,
            CancellationToken ctx)
        {
            AzureApp? app;
            if (applicationId.HasValue && applicationSecret is { })
            {
                app = AzureApp.GetApp(applicationId.GetValueOrDefault(), tenantId);
                if (app is null)
                {
                    AzureApp.AddApp(applicationId.Value, tenantId, applicationSecret);
                }
            }
            else
            {
                // check if app already exist for this tenant/directory.
                app = AzureApp.GetApp(tenantId).FirstOrDefault();

                if (app is null)
                {
                    return this.BadRequest("No Application found for this tenant");
                }
            }

            if (await this.subscriptionManager.RegisterSubscriptionAsync(tenantId, resource, resourceUrl, ctx)
                .ConfigureAwait(false))
            {
                return this.Ok("Success.");
            }
            else
            {
                return this.BadRequest("Error");
            }
        }

        /// <summary>
        /// Endpoint to remove the subscription.
        /// </summary>
        /// <param name="tenantId">The Id of the Tenant.</param>
        /// <param name="resource">The resource for the webhook.</param>
        /// <param name="ctx">The cancelation token.</param>
        /// <returns>Returns success or failure.</returns>
        [HttpGet]
        [Route("remove")]
        public async Task<ActionResult<string>> RemoveGet([FromQuery] Guid tenantId, [FromQuery] SubscriptionResource resource, CancellationToken ctx)
        {
            if (await this.subscriptionManager.RemoveSubscriptionAsync(tenantId, resource, ctx)
                .ConfigureAwait(false))
            {
                return this.Ok("Success");
            }
            else
            {
                return this.BadRequest("Error");
            }
        }
    }
}
