// <copyright file="CallRecordArgs.cs" company="NiCE IT Management Solutions GmbH">
// Copyright (c) NiCE IT Management Solutions GmbH. All rights reserved.
// </copyright>

namespace NiCE.Community.TeamsCallRecordWebhook.Subscription
{
    using System;

    /// <summary>
    /// Call record arguments.
    /// </summary>
    public class CallRecordArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CallRecordArgs"/> class.
        /// </summary>
        /// <param name="directoryId">The directory ID.</param>
        /// <param name="callRecordId">The call record ID.</param>
        public CallRecordArgs(Guid directoryId, Guid callRecordId)
        {
            this.DirectoryId = directoryId;
            this.CallRecordId = callRecordId;
        }

        /// <summary>
        /// Gets the directory id.
        /// </summary>
        public Guid DirectoryId { get; }

        /// <summary>
        /// gets the call record id.
        /// </summary>
        public Guid CallRecordId { get; }
    }
}
