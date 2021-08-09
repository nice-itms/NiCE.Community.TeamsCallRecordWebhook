// <copyright file="AzureApp.cs" company="NiCE IT Management Solutions GmbH">
// Copyright (c) NiCE IT Management Solutions GmbH. All rights reserved.
// </copyright>

namespace NiCE.A365.Collector.Authentication
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Azure.Identity;
    using Microsoft.Graph;

    /// <summary>
    /// The main Azure App.
    /// </summary>
    public class AzureApp
    {
        private static readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, AzureApp>> AzureApps = new ();

        private AzureApp(Guid applicationId, Guid directoryId, string secret)
        {
            this.ApplicationId = applicationId;
            this.DirectoryId = directoryId;
            this.Secret = secret;
        }

        /// <summary>
        /// Gets the id of the Application.
        /// </summary>
        public Guid ApplicationId { get; }

        /// <summary>
        /// Gets the id of the Directory.
        /// </summary>
        public Guid DirectoryId { get; }

        /// <summary>
        /// Gets the Application secret.
        /// </summary>
        public string Secret { get; }

        /// <summary>
        /// Adds the Azure App.
        /// </summary>
        /// <param name="applicationId">The id of the application.</param>
        /// <param name="directoryId">The Id of the Directory.</param>
        /// <param name="secret">The application secret.</param>
        /// <returns>returns the application.</returns>
        public static AzureApp AddApp(Guid applicationId, Guid directoryId, string secret)
        {
            // app list
            var applications = AzureApps.GetOrAdd(
                applicationId,
                s => new ConcurrentDictionary<Guid, AzureApp>());

            // tenant list
            var application = applications.GetOrAdd(directoryId, s => new AzureApp(applicationId, directoryId, secret));

            return application;
        }

        /// <summary>
        /// Gets the app for a application and directory..
        /// </summary>
        /// <param name="applicationId">The id of the application.</param>
        /// <param name="directoryId">The id of the directory.</param>
        /// <returns>returns the app.</returns>
        public static AzureApp? GetApp(Guid applicationId, Guid directoryId)
        {
            return AzureApps.TryGetValue(applicationId, out var applications) &&
                   applications.TryGetValue(directoryId, out var application)
                ? application
                : null;
        }

        /// <summary>
        /// Gets the app for a tenant.
        /// </summary>
        /// <param name="tenantId">The id of the tenant.</param>
        /// <returns>returns the app.</returns>
        public static IEnumerable<AzureApp> GetApp(Guid tenantId)
        {
            foreach (var (_, apps) in AzureApps)
            {
                if (apps.TryGetValue(tenantId, out var app))
                {
                    yield return app;
                }
            }
        }

        /// <summary>
        /// Gets the Graph Service Client.
        /// </summary>
        /// <returns>returns the Graph service client.</returns>
        public GraphServiceClient GetGraphServiceClient()
        {
            var clientCredentialProvider = new ClientSecretCredential(this.DirectoryId.ToString(), this.ApplicationId.ToString(), this.Secret);

            // Create the Graph Client
            return new GraphServiceClient(clientCredentialProvider);
        }
    }
}
