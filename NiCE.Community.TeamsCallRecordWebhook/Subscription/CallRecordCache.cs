// <copyright file="CallRecordCache.cs" company="NiCE IT Management Solutions GmbH">
// Copyright (c) NiCE IT Management Solutions GmbH. All rights reserved.
// </copyright>

namespace NiCE.Community.TeamsCallRecordWebhook
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Microsoft.Graph;
    using Microsoft.Graph.CallRecords;
    using NiCE.A365.Collector.Authentication;
    using NiCE.Community.TeamsCallRecordWebhook.Interfaces;
    using NiCE.Community.TeamsCallRecordWebhook.Subscription;

    /// <summary>
    /// Cache the Call Record Data.
    /// </summary>
    public class CallRecordCache
    {
        private readonly ISubscriptionManager subscriptionManager;
        private readonly ILogger<CallRecordCache> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallRecordCache"/> class.
        /// </summary>
        /// <param name="logger">Logger to log event.</param>
        /// <param name="subscriptionManager">The subscription Manager manages the subscription.</param>
        public CallRecordCache(ILogger<CallRecordCache> logger, ISubscriptionManager subscriptionManager)
        {
            this.subscriptionManager = subscriptionManager;
            this.logger = logger;

            subscriptionManager.CallRecordCreated += this.OnNewCallRecord;

            this.CallRecords = new List<CallRecord>();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="CallRecordCache"/> class.
        /// </summary>
        ~CallRecordCache()
        {
            this.subscriptionManager.CallRecordCreated -= this.OnNewCallRecord;
        }

        /// <summary>
        /// Gets the List of Call Record Data.
        /// </summary>
        public List<CallRecord> CallRecords { get; }

        private async void OnNewCallRecord(object? sender, CallRecordArgs args)
        {
            try
            {
                // get application data.
                var app = AzureApp.GetApp(args.DirectoryId).FirstOrDefault();

                if (app is null)
                {
                    throw new KeyNotFoundException($"No application was register for the directory {args.DirectoryId}.");
                }

                // Get graph client
                var client = app.GetGraphServiceClient();

                var request = client.Communications.CallRecords[args.CallRecordId.ToString()].Request();
                request.QueryOptions.Add(new QueryOption("$expand", "sessions($expand=segments)"));
                var record = await request.GetAsync().ConfigureAwait(false);

                if (record is { })
                {
                    this.CallRecords.Add(record);
                }
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Can't progress CallRecord");
            }
        }
    }
}
