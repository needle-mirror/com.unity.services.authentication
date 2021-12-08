using System;
using Unity.Services.Authentication.Internal;

namespace Unity.Services.Authentication
{
    class PlayerIdComponent : IPlayerId
    {
        public event Action<string> PlayerIdChanged;
        public string PlayerId { get; internal set; }

        IAuthenticationService m_AuthenticationService;

        public PlayerIdComponent(IAuthenticationService service)
        {
            m_AuthenticationService = service;
            m_AuthenticationService.SignedIn += () => ValidatePlayerChanged();
            m_AuthenticationService.SignedOut += () => ValidatePlayerChanged();
            m_AuthenticationService.SignInFailed += (e) => ValidatePlayerChanged();
        }

        void ValidatePlayerChanged()
        {
            if (PlayerId != m_AuthenticationService.PlayerId)
            {
                PlayerId = m_AuthenticationService.PlayerId;
                PlayerIdChanged?.Invoke(PlayerId);
            }
        }
    }
}
