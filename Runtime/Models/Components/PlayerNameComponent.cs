using System;
using Unity.Services.Authentication.Internal;

namespace Unity.Services.Authentication
{
    class PlayerNameComponent : IPlayerName, IPlayerNameComponent
    {
        const string k_CacheKey = "player_name";
        string m_PlayerName;

        public event Action<string> PlayerNameChanged;

        public string PlayerName { get => m_PlayerName; internal set => SetPlayerName(value); }

        readonly IAuthenticationCache m_Cache;

        internal PlayerNameComponent(IAuthenticationCache cache)
        {
            m_Cache = cache;
            m_PlayerName = GetPlayerNameFromCache();
        }

        internal void Clear()
        {
            SetPlayerName(null);
        }

        internal void Refresh()
        {
            SetPlayerName(GetPlayerNameFromCache());
        }

        string GetPlayerNameFromCache()
        {
            return m_Cache.GetString(k_CacheKey);
        }

        void SetPlayerName(string playerName)
        {
            if (PlayerName != playerName)
            {
                m_PlayerName = playerName;

                if (m_PlayerName == null)
                {
                    m_Cache.DeleteKey(k_CacheKey);
                }
                else
                {
                    m_Cache.SetString(k_CacheKey, m_PlayerName);
                }

                try
                {
                    PlayerNameChanged?.Invoke(playerName);
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }
            }
        }
    }
}
