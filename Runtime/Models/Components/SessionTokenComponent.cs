namespace Unity.Services.Authentication
{
    class SessionTokenComponent
    {
        const string k_CacheKey = "session_token";

        internal string SessionToken { get => GetSessionToken(); set => SetSessionToken(value); }

        readonly IAuthenticationCache m_Cache;

        internal SessionTokenComponent(IAuthenticationCache cache)
        {
            m_Cache = cache;
        }

        internal void Clear()
        {
            m_Cache.DeleteKey(k_CacheKey);
        }

        internal void Migrate()
        {
            m_Cache.Migrate(k_CacheKey);
        }

        string GetSessionToken()
        {
            return m_Cache.GetString(k_CacheKey);
        }

        void SetSessionToken(string sessionToken)
        {
            m_Cache.SetString(k_CacheKey, sessionToken);
        }
    }
}
