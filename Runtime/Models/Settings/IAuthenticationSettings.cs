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
    }
}
