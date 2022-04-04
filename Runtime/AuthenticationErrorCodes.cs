using Unity.Services.Core;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// AuthenticationErrorCodes lists the error codes to expect from <c>AuthenticationException</c> and failed events.
    /// The error code range is: 10000 to 10999.
    /// </summary>
    public static class AuthenticationErrorCodes
    {
        /// <summary>
        /// The minimal value of an Authentication error code. Any error code thrown from Authentication SDK less than
        /// it is from <see cref="CommonErrorCodes"/>.
        /// </summary>
        public static readonly int MinValue = 10000;

        /// <summary>
        /// A client error that is returned when the user is not in the right state.
        /// For example, calling SignOut when the user is already signed out will result in this error.
        /// </summary>
        public static readonly int ClientInvalidUserState = 10000;

        /// <summary>
        /// A client error that is returned when trying to sign-in with the session token while there is no cached
        /// session token.
        /// </summary>
        public static readonly int ClientNoActiveSession = 10001;

        /// <summary>
        /// The error returned when the parameter is missing or not in the right format.
        /// </summary>
        public static readonly int InvalidParameters = 10002;

        /// <summary>
        /// The error returned when a player tries to link a social account that is already linked with another player.
        /// </summary>
        public static readonly int AccountAlreadyLinked = 10003;

        /// <summary>
        /// The error returned when a player tries to link a social account but this player has already reached the limit of links for that account type.
        /// Social accounts linking are typically limited to one link per type per player.
        /// </summary>
        public static readonly int AccountLinkLimitExceeded = 10004;

        /// <summary>
        /// The error returned when a player tries to unlink a social account but no external id for that provider is found for the account.
        /// This could be because the player info has not been loaded.
        /// </summary>
        public static readonly int ClientUnlinkExternalIdNotFound = 10005;

        /// <summary>
        /// The error returned when a player tries to switch profile but the profile name is invalid.
        /// </summary>
        public static readonly int ClientInvalidProfile = 10006;

        /// <summary>
        /// The error returned when a session token is invalid
        /// </summary>
        public static readonly int InvalidSessionToken = 10007;
    }
}
