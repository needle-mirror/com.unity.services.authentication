namespace Unity.Services.Authentication
{
    interface IAuthenticationSettings
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

        /// <summary>
        /// The max retries to get well known keys from server.
        /// </summary>
        int WellKnownKeysMaxAttempt { get; }
    }

    class AuthenticationSettings : IAuthenticationSettings
    {
        const int k_AccessTokenRefreshBuffer = 300;
        const int k_AccessTokenExpiryBuffer = 15;
        const int k_RefreshAttemptFrequency = 30;
        const int k_WellKnownKeysMaxAttempt = 3;

        public int AccessTokenRefreshBuffer => k_AccessTokenRefreshBuffer;
        public int AccessTokenExpiryBuffer => k_AccessTokenExpiryBuffer;
        public int RefreshAttemptFrequency => k_RefreshAttemptFrequency;
        public int WellKnownKeysMaxAttempt => k_WellKnownKeysMaxAttempt;
    }
}
