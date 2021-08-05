using System;
using Unity.Services.Authentication.Internal;
using Unity.Services.Core;

namespace Unity.Services.Authentication
{
    class PlayerIdComponent : IPlayerId
    {
        IAuthenticationService m_AuthenticationService;

        public string PlayerId => m_AuthenticationService.PlayerId;
        public event Action<string> PlayerIdChanged;

        public PlayerIdComponent(IAuthenticationService service)
        {
            m_AuthenticationService = service;
            m_AuthenticationService.SignedIn += OnAuthenticationSignedIn;
            m_AuthenticationService.SignedOut += OnAuthenticationSignedOut;
            m_AuthenticationService.SignInFailed += OnAuthenticationSignInFailed;
        }

        void OnAuthenticationSignInFailed(RequestFailedException error)
        {
            NotifyPlayerChanged();
        }

        void OnAuthenticationSignedOut()
        {
            NotifyPlayerChanged();
        }

        void OnAuthenticationSignedIn()
        {
            NotifyPlayerChanged();
        }

        void NotifyPlayerChanged()
        {
            PlayerIdChanged?.Invoke(PlayerId);
        }
    }
}
