using System;
using Unity.Services.Authentication.Internal;

namespace Unity.Services.Authentication
{
    class PlayerIdComponent : IPlayerId
    {
        const string k_CacheKey = "player_id";

        public event Action<string> PlayerIdChanged;

        public string PlayerId { get => GetPlayerId(); internal set => SetPlayerId(value); }

        readonly IAuthenticationCache m_Cache;

        internal PlayerIdComponent(IAuthenticationCache cache)
        {
            m_Cache = cache;
        }

        internal void Clear()
        {
            m_Cache.DeleteKey(k_CacheKey);
        }

        string GetPlayerId()
        {
            return m_Cache.GetString(k_CacheKey);
        }

        void SetPlayerId(string playerId)
        {
            if (PlayerId != playerId)
            {
                m_Cache.SetString(k_CacheKey, playerId);
                PlayerIdChanged?.Invoke(playerId);
            }
        }
    }
}
