namespace Unity.Services.Authentication.Server
{
    /// <summary>
    /// Enum representing the authentication state of the server.
    /// </summary>
    public enum ServerAuthenticationState
    {
        /// <summary>
        /// The server is currently authorized.
        /// </summary>
        Authorized,

        /// <summary>
        /// The server is currently in the process of signing in.
        /// </summary>
        SigningIn,

        /// <summary>
        /// The server's authentication has expired.
        /// </summary>
        Expired,

        /// <summary>
        /// The server is currently refreshing its authentication.
        /// </summary>
        Refreshing,

        /// <summary>
        /// The server is not authorized.
        /// </summary>
        Unauthorized
    }
}
