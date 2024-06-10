using System;
using Unity.Services.Authentication.Internal;

namespace Unity.Services.Authentication
{
    class PlayerIdComponent : IPlayerId
    {
        const string k_CacheKey = "player_id";
        string m_PlayerId;
        public event Action<string> PlayerIdChanged;

        public string PlayerId { get => m_PlayerId; internal set => SetPlayerId(value); }

        readonly IAuthenticationCache m_Cache;

        internal PlayerIdComponent(IAuthenticationCache cache)
        {
            m_Cache = cache;
            m_PlayerId = GetPlayerId();
        }

        internal void Clear()
        {
            m_PlayerId = null;
            m_Cache.DeleteKey(k_CacheKey);
        }

        internal void Refresh()
        {
            m_PlayerId = GetPlayerId();
        }

        string GetPlayerId()
        {
            return m_Cache.GetString(k_CacheKey);
        }

        void SetPlayerId(string playerId)
        {
            if (PlayerId != playerId)
            {
                m_PlayerId = playerId;
                m_Cache.SetString(k_CacheKey, playerId);
                try
                {
                    PlayerIdChanged?.Invoke(playerId);
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }
            }
        }
    }
}
