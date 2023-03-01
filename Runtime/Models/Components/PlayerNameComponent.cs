using System;
using Unity.Services.Authentication.Internal;

namespace Unity.Services.Authentication
{
    class PlayerNameComponent : IPlayerName
    {
        const string k_CacheKey = "player_name";

        public event Action<string> PlayerNameChanged;

        public string PlayerName { get => GetPlayerName(); internal set => SetPlayerName(value); }

        readonly IAuthenticationCache m_Cache;

        internal PlayerNameComponent(IAuthenticationCache cache)
        {
            m_Cache = cache;
        }

        internal void Clear()
        {
            m_Cache.DeleteKey(k_CacheKey);
        }

        string GetPlayerName()
        {
            return m_Cache.GetString(k_CacheKey);
        }

        void SetPlayerName(string playerName)
        {
            if (PlayerName != playerName)
            {
                m_Cache.SetString(k_CacheKey, playerName);
                PlayerNameChanged?.Invoke(playerName);
            }
        }
    }
}
