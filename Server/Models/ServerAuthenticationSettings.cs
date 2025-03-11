namespace Unity.Services.Authentication.Server
{
    interface IServerAuthenticationSettings
    {
        /// <summary>
        /// The buffer time in seconds to start access token refresh before the access token expires.
        /// </summary>
        int AccessTokenRefreshBuffer { get; }

        /// <summary>
        /// The buffer time in seconds to treat token as expired before the token's expiry time.
        /// This is to deal with the time difference between the client and server.
        /// </summary>
        int AccessTokenExpiryBuffer { get; }

        /// <summary>
        /// The time in seconds between access token refresh retries.
        /// </summary>
        int RefreshAttemptFrequency { get; }
    }

    class ServerAuthenticationSettings : IServerAuthenticationSettings
    {
        const int k_AccessTokenRefreshBuffer = 300;
        const int k_AccessTokenExpiryBuffer = 15;
        const int k_RefreshAttemptFrequency = 30;

        public int AccessTokenRefreshBuffer { get; internal set; }
        public int AccessTokenExpiryBuffer { get; internal set; }
        public int RefreshAttemptFrequency { get; internal set; }

        internal ServerAuthenticationSettings()
        {
            AccessTokenRefreshBuffer = k_AccessTokenRefreshBuffer;
            AccessTokenExpiryBuffer = k_AccessTokenExpiryBuffer;
            RefreshAttemptFrequency = k_RefreshAttemptFrequency;
        }

        internal void Reset()
        {
            AccessTokenRefreshBuffer = k_AccessTokenRefreshBuffer;
            AccessTokenExpiryBuffer = k_AccessTokenExpiryBuffer;
            RefreshAttemptFrequency = k_RefreshAttemptFrequency;
        }
    }
}
