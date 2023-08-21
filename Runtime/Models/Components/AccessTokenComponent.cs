using System;
using Unity.Services.Authentication.Internal;

namespace Unity.Services.Authentication
{
    class AccessTokenComponent : IAccessToken, IAccessTokenObserver
    {
        public event Action<string> AccessTokenChanged;

        public string AccessToken
        {
            get => m_AccessToken;
            internal set => SetAccessToken(value);
        }

        public DateTime? ExpiryTime { get; internal set; }

        string m_AccessToken;

        internal AccessTokenComponent()
        {
        }

        internal void Clear()
        {
            AccessToken = null;
            ExpiryTime = null;
        }

        void SetAccessToken(string accessToken)
        {
            if (m_AccessToken != accessToken)
            {
                m_AccessToken = accessToken;
                AccessTokenChanged?.Invoke(m_AccessToken);
            }
        }
    }
}
