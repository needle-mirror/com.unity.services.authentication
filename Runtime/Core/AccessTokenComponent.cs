using Unity.Services.Authentication.Internal;

namespace Unity.Services.Authentication
{
    class AccessTokenComponent : IAccessToken
    {
        IAuthenticationService m_AuthenticationService;

        public string AccessToken => m_AuthenticationService.AccessToken;

        public AccessTokenComponent(IAuthenticationService service)
        {
            m_AuthenticationService = service;
        }
    }
}
