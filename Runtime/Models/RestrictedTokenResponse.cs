namespace Unity.Services.Authentication
{
    /// <summary>
    /// Response from <see cref="IAuthenticationService.GenerateRestrictedTokenAsync"/>.
    /// </summary>
    public sealed class RestrictedTokenResponse
    {
        /// <summary>
        /// The player ID the issued tokens belong to.
        /// </summary>
        public string UserId { get; internal set; }

        /// <summary>
        /// The issued ID token.
        /// </summary>
        public string IdToken { get; internal set; }

        /// <summary>
        /// The issued session token.
        /// </summary>
        public string SessionToken { get; internal set; }

        /// <summary>
        /// Lifetime of the issued ID token, in seconds.
        /// </summary>
        public int ExpiresIn { get; internal set; }
    }
}
