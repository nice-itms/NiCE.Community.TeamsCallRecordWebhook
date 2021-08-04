// <copyright file="SubscriptionResource.cs" company="NiCE IT Management Solutions GmbH">
// Copyright (c) NiCE IT Management Solutions GmbH. All rights reserved.
// </copyright>

namespace NiCE.Community.TeamsCallRecordWebhook.Subscription
{
    /// <summary>
    /// The Resources we can Subscribe to.
    /// </summary>
    public enum SubscriptionResource
    {
        /// <summary>
        /// <para>
        ///     Subscribe to Security alerts. https://docs.microsoft.com/en-us/graph/api/resources/alert
        /// </para>
        /// <para>
        ///     Changes to a specific alert: /security/alerts/{id}<br/>
        ///     Changes to filtered alerts: /security/alerts/?$filter
        /// </para>
        /// </summary>
        Alert,

        /// <summary>
        /// <para>
        ///     Subscribe to Teams Call records. https://docs.microsoft.com/en-us/graph/api/resources/callrecords-callrecord
        /// </para>
        /// <para>
        ///     Changes to all call records: /communications/callRecord
        /// </para>
        /// </summary>
        CallRecord,

        /// <summary>
        /// <para>
        ///     Subscribe to Teams chat messages. https://docs.microsoft.com/en-us/graph/api/resources/chatmessage
        /// </para>
        /// <para>
        ///     Changes to chat messages in all channels in all teams: /teams/getAllMessages<br/>
        ///     Changes to chat messages in a specific channel: /teams/{id}/channels/{id}/messages<br/>
        ///     Changes to chat messages in all chats: /chats/getAllMessages<br/>
        ///     Changes to chat messages in a specific chat: /chats/{id}/messages
        /// </para>
        /// </summary>
        ChatMessage,

        /// <summary>
        /// <para>
        ///     Subscribe to DriveItem on OneDrive for Business. https://docs.microsoft.com/en-us/graph/api/resources/driveitem
        /// </para>
        /// <para>
        ///     Changes to content within the hierarchy of the root folder:<br/>
        ///     - /drives/{id}/root<br/>
        ///     - /users/{id}/drive/root
        /// </para>
        /// </summary>
        DriveItemBusiness,

        /// <summary>
        /// <para>
        ///     Subscribe to DriveItem on OneDrive (personal). https://docs.microsoft.com/en-us/graph/api/resources/driveitem
        /// </para>
        /// <para>
        ///     Changes to content within the hierarchy of any folder: /users/{id}/drive/root
        /// </para>
        /// </summary>
        DriveItemPersonal,

        /// <summary>
        /// <para>
        ///     Subscribe to Group. https://docs.microsoft.com/en-us/graph/api/resources/group
        /// </para>
        /// <para>
        ///     Changes to all groups: /groups<br/>
        ///     Changes to a specific group: /groups/{id}<br/>
        ///     Changes to owners of a specific group: /groups/{id}/owners<br/>
        ///     Changes to members of a specific group: /groups/{id}/members
        /// </para>
        /// </summary>
        Group,

        /// <summary>
        /// <para>
        ///     Subscribe to Outlook message. https://docs.microsoft.com/en-us/graph/api/resources/message
        /// </para>
        /// <para>
        ///      Changes to all messages in a user's mailbox: /users/{id}/messages<br/>
        ///      Changes to messages in a user's Inbox: /users/{id}/mailFolders('inbox')/messages
        /// </para>
        /// </summary>
        Message,

        /// <summary>
        /// <para>
        ///     Subscribe to list under a SharePoint site. https://docs.microsoft.com/en-us/graph/api/resources/list
        /// </para>
        /// <para>
        ///     Changes to content within the list: /sites/{id}/lists/{id}
        /// </para>
        /// </summary>
        SharePointSiteList,

        /// <summary>
        /// <para>
        ///     Subscribe to User. https://docs.microsoft.com/en-us/graph/api/resources/user
        /// </para>
        /// <para>
        ///     Changes to all users: /users<br/>
        ///     Changes to a specific user: /users/{id}
        /// </para>
        /// </summary>
        User,
    }
}