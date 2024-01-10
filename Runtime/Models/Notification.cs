using System;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// Contains Notification Information
    /// </summary>
    public struct Notification
    {
        /// <summary>
        /// Notification ID on the server
        /// </summary>
        public string Id;
        /// <summary>
        /// Case ID for the DSA notification in case the player wants to request more details
        /// </summary>
        public string CaseId;
        /// <summary>
        /// Message to be displayed to the player
        /// </summary>
        public string Message;
        /// <summary>
        /// The player ID for easy access if the player wants to request more details
        /// </summary>
        public string PlayerId;
        /// <summary>
        /// The project ID for easy access if the player wants to request more details
        /// </summary>
        public string ProjectId;
        /// <summary>
        /// The notification type, currently only supports DSA notifications
        /// </summary>
        public string Type;
        /// <summary>
        /// Date the notification was created is Unix format.
        /// </summary>
        public string CreatedAt;
    }
}
