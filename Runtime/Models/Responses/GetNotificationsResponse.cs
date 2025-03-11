using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    [Serializable]
    class GetNotificationsResponse
    {
        [Preserve]
        public GetNotificationsResponse() {}

        [JsonProperty("notifications")]
        public List<NotificationResponse> Notifications;

        public List<Notification> ToNotificationList()
        {
            if (Notifications.Count < 1)
            {
                return null;
            }
            var notifications = new List<Notification>();
            foreach (var responseNotification in Notifications)
            {
                notifications.Add(new Notification
                {
                    Id = responseNotification.Id,
                    CaseId = responseNotification.CaseId,
                    CreatedAt = responseNotification.CreatedAt,
                    Message = responseNotification.Message,
                    PlayerId = responseNotification.PlayerId,
                    ProjectId = responseNotification.ProjectId,
                    Type = responseNotification.Type,
                });
            }

            return notifications;
        }
    }

    [Serializable]
    class NotificationResponse
    {
        [Preserve]
        public NotificationResponse() {}

        [JsonProperty("id")]
        public string Id;
        [JsonProperty("caseID")]
        public string CaseId;
        [JsonProperty("message")]
        public string Message;
        [JsonProperty("playerId")]
        public string PlayerId;
        [JsonProperty("projectId")]
        public string ProjectId;
        [JsonProperty("type")]
        public string Type;
        [JsonProperty("createdAt")]
        public string CreatedAt;
    }
}
