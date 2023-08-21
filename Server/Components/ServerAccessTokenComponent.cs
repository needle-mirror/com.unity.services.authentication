using System;
using Unity.Services.Authentication.Server.Internal;

namespace Unity.Services.Authentication.Server
{
    class ServerAccessTokenComponent : IServerAccessToken
    {
        public event Action<string> AccessTokenChanged;

        public string AccessToken
        {
            get => m_AccessToken;
            internal set => SetAccessToken(value);
        }

        public DateTime? ExpiryTime { get; internal set; }

        string m_AccessToken;

        internal ServerAccessTokenComponent()
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
