using System;
using Unity.Services.Authentication.Internal;

namespace Unity.Services.Authentication
{
    class PlayerNameComponent : IPlayerName
    {
        const string k_CacheKey = "player_name";
        string m_PlayerName;

        public event Action<string> PlayerNameChanged;

        public string PlayerName { get => m_PlayerName; internal set => SetPlayerName(value); }

        readonly IAuthenticationCache m_Cache;

        internal PlayerNameComponent(IAuthenticationCache cache)
        {
            m_Cache = cache;
            m_PlayerName = GetPlayerName();
        }

        internal void Clear()
        {
            m_PlayerName = null;
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
                m_PlayerName = playerName;
                m_Cache.SetString(k_CacheKey, playerName);
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
