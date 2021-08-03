// <copyright file="ISubscriptionManager.cs" company="NiCE IT Management Solutions GmbH">
// Copyright (c) NiCE IT Management Solutions GmbH. All rights reserved.
// </copyright>

namespace NiCE.Community.TeamsCallRecordWebhook.Interfaces
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using NiCE.Community.TeamsCallRecordWebhook.Subscription;

    /// <summary>
    /// Manager to handle subscriptions.
    /// </summary>
    public interface ISubscriptionManager
    {
        /// <summary>
        /// Event when Call record is created.
        /// </summary>
        event EventHandler<CallRecordArgs>? CallRecordCreated;

        /// <summary>
        /// Event when Call record is updated.
        /// </summary>
        event EventHandler<CallRecordArgs>? CallRecordUpdated;

        /// <summary>
        /// Event when Call record is deleted.
        /// </summary>
        event EventHandler<CallRecordArgs>? CallRecordDeleted;

        /// <summary>
        /// Processing the notification.
        /// </summary>
        /// <param name="request"><see cref="HttpRequest"/> added by DI.</param>
        /// <param name="bodyData">Body of the request.</param>
        /// <param name="validationToken"> The validation token.</param>
        /// <returns>returns ok of failure.</returns>
        ActionResult<string> ProcessNotification(HttpRequest request, string bodyData, string? validationToken = null);

        /// <summary>
        /// Processing the subscription.
        /// </summary>
        /// <param name="directoryId"> The id of the directory.</param>
        /// <param name="resource"><see cref="SubscriptionResource"/> added by DI.</param>
        /// <param name="resourcePath"> The path to the resource.</param>
        /// <param name="ctx"><see cref="CancellationToken"/> added by DI.</param>
        /// <returns>returns ok of failure.</returns>
        Task<bool> RegisterSubscriptionAsync(Guid directoryId, SubscriptionResource resource, string resourcePath, CancellationToken ctx = default);

        /// <summary>
        /// Processing the subscription removal.
        /// </summary>
        /// <param name="directoryId"> The id of the directory.</param>
        /// <param name="resource"><see cref="SubscriptionResource"/> added by DI.</param>
        /// <param name="ctx"><see cref="CancellationToken"/> added by DI.</param>
        /// <returns>returns ok of failure.</returns>
        Task<bool> RemoveSubscriptionAsync(Guid directoryId, SubscriptionResource resource, CancellationToken ctx = default);
    }
}