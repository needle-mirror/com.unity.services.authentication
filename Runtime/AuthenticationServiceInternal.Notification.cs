using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unity.Services.Authentication
{
    partial class AuthenticationServiceInternal
    {
        public List<Notification> Notifications => m_Notifications;

        List<Notification> m_Notifications;

        public async Task<List<Notification>> GetNotificationsAsync()
        {
            if (!IsAuthorized)
            {
                throw ExceptionHandler.BuildClientInvalidStateException(State);
            }

            try
            {
                var response = await NetworkClient.GetNotificationsAsync(PlayerId);
                m_Notifications = response.ToNotificationList();
                return m_Notifications;
            }
            catch (WebRequestException e)
            {
                throw ExceptionHandler.ConvertException(e);
            }
        }
    }
}
