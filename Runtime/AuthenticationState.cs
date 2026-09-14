namespace Unity.Services.Authentication
{
    /// <summary>
    /// The states the authentication service can be in for the current player,
    /// reflecting the lifecycle of the player's session and access token.
    /// </summary>
    public enum AuthenticationState
    {
        /// <summary>
        /// The player is not signed in. This is the initial state and the state after signing out.
        /// </summary>
        SignedOut,

        /// <summary>
        /// A sign-in operation is in progress and has not yet completed.
        /// </summary>
        SigningIn,

        /// <summary>
        /// The player is signed in and holds a valid, unexpired access token.
        /// </summary>
        Authorized,

        /// <summary>
        /// The player is signed in and the access token is currently being refreshed.
        /// </summary>
        Refreshing,

        /// <summary>
        /// The player is signed in but the session has expired, i.e. the access token expired and was not refreshed.
        /// </summary>
        Expired
    }
}
