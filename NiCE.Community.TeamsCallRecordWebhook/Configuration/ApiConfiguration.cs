// <copyright file="ApiConfiguration.cs" company="NiCE IT Management Solutions GmbH">
// Copyright (c) NiCE IT Management Solutions GmbH. All rights reserved.
// </copyright>

namespace NiCE.Community.TeamsCallRecordWebhook.Configuration
{
    /// <summary>
    /// The configuration of the Api.
    /// </summary>
    public class ApiConfiguration
    {
        // public string Thumbprint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the endpoint to be called for the subscription resource. Standard set for teams call records.
        /// </summary>
        public string PublicEndpoint { get; set; } = "/communications/callRecords";
    }
}
