// <copyright file="SubscriptionCache.cs" company="NiCE IT Management Solutions GmbH">
// Copyright (c) NiCE IT Management Solutions GmbH. All rights reserved.
// </copyright>

namespace NiCE.Community.TeamsCallRecordWebhook.Subscription
{
    using System;

    /// <summary>
    /// The subscription cache.
    /// </summary>
    public class SubscriptionCache
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionCache"/> class.
        /// </summary>
        /// <param name="id">The cache id.</param>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="resource"><see cref="SubscriptionResource"/> added by DI.</param>
        /// <param name="resourcePath">The resource path.</param>
        /// <param name="expirationDateTime">The expiration date.</param>
        public SubscriptionCache(Guid id, Guid tenantId, SubscriptionResource resource, string resourcePath, DateTime expirationDateTime)
        {
            this.Id = id;
            this.TenantId = tenantId;
            this.Resource = resource;
            this.ResourcePath = resourcePath;
            this.ExpirationDateTime = expirationDateTime;
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the tenant id.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the subscription resource.
        /// </summary>
        public SubscriptionResource Resource { get; set; }

        /// <summary>
        /// Gets or sets the resource path.
        /// </summary>
        public string ResourcePath { get; set; }

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        public DateTime ExpirationDateTime { get; set; }
    }
}
