namespace Unity.Services.Authentication
{
    class SessionTokenComponent
    {
        const string k_CacheKey = "session_token";
        string m_SessionToken;

        internal string SessionToken { get => m_SessionToken; set => SetSessionToken(value); }

        readonly IAuthenticationCache m_Cache;

        internal SessionTokenComponent(IAuthenticationCache cache)
        {
            m_Cache = cache;
            m_SessionToken = GetSessionToken();
        }

        internal void Clear()
        {
            m_SessionToken = null;
            m_Cache.DeleteKey(k_CacheKey);
        }

        internal void Migrate()
        {
            m_Cache.Migrate(k_CacheKey);
        }

        internal void Refresh()
        {
            m_SessionToken = GetSessionToken();
        }

        string GetSessionToken()
        {
            return m_Cache.GetString(k_CacheKey);
        }

        void SetSessionToken(string sessionToken)
        {
            m_SessionToken = sessionToken;
            m_Cache.SetString(k_CacheKey, sessionToken);
        }
    }
}
