using Unity.Services.Core;

namespace Unity.Services.Authentication.Server
{
    /// <summary>
    /// ServerAuthenticationErrorCodes lists the error codes to expect from <c>ServerAuthenticationException</c> and failed events.
    /// The error code range is the same as AuthenticationException: 10000 to 10999.
    /// </summary>
    public static class ServerAuthenticationErrorCodes
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
        /// The error returned when the parameter is missing or not in the right format.
        /// </summary>
        public static readonly int InvalidParameters = 10002;
    }
}
