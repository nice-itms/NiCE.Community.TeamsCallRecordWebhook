// <copyright file="Worker.cs" company="NiCE IT Management Solutions GmbH">
// Copyright (c) NiCE IT Management Solutions GmbH. All rights reserved.
// </copyright>

namespace NiCE.Community.TeamsCallRecordWebhook
{
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using NiCE.Community.TeamsCallRecordWebhook.Interfaces;

    /// <summary>
    /// Service worker.
    /// </summary>
    public class Worker : IHostedService
    {
        private readonly ILogger<Worker> logger;

        private readonly ISubscriptionManager subscriptionManager;

        private readonly ApplicationPartManager partManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="Worker"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger{TCategoryName}"/> added by DI.</param>
        /// <param name="cache"><see cref="CallRecordCache"/> added by DI.</param>
        /// <param name="subscriptionManager"><see cref="ISubscriptionManager"/> added by DI.</param>
        /// <param name="partManager"><see cref="ApplicationPartManager"/> added by DI.</param>
        public Worker(ILogger<Worker> logger, CallRecordCache cache, ISubscriptionManager subscriptionManager, ApplicationPartManager partManager)
        {
            this.logger = logger;
            this.subscriptionManager = subscriptionManager;
            this.partManager = partManager;
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"##################################");
            this.logger.LogInformation($"Please Register Subscription if CallRecord should be received by the webhook. No data is saved by the application on the system.");
            this.logger.LogInformation($"##################################");

            var controllerFeature = new ControllerFeature();
            this.partManager.PopulateFeature(controllerFeature);
            var nameList = controllerFeature.Controllers.Select(e => e.FullName).ToList();
            this.logger.LogInformation("Enable Controllers: {Controllers}", JsonSerializer.Serialize(nameList));

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
