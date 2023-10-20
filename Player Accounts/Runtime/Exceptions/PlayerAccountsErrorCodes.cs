namespace Unity.Services.Authentication.PlayerAccounts
{
    /// <summary>
    /// PlayerAccountsErrorCodes lists the error codes from <c>PlayerAccountsException</c> and failed events.
    /// The error code range is : 10100 to 10199.
    /// </summary>
    public static class PlayerAccountsErrorCodes
    {
        /// <summary>
        /// Unknown error
        /// </summary>
        public const int UnknownError = 10100;

        /// <summary>
        /// A client error that is returned when the user is not in the right state.
        /// For example, calling SignOut when the user is already signed out will result in this error.
        /// </summary>
        public const int InvalidState = 10101;

        /// <summary>
        /// The client Id has not been configured in the editor
        /// </summary>
        public const int MissingClientId = 10102;

        /// <summary>
        /// InvalidClient Client authentication failed (e.g., unknown client, no client authentication included,
        /// or unsupported authentication method).  The authorization server MAY return an HTTP 401 (Unauthorized) status code
        /// to indicate which HTTP authentication schemes are supported.  If the client attempted to authenticate via the
        /// "Authorization" request header field, the authorization server MUST respond with an HTTP 401 (Unauthorized) status code
        /// and include the "WWW-Authenticate" response header field matching the authentication scheme used by the client.
        /// </summary>
        public const int InvalidClient = 10103;

        /// <summary>
        /// InvalidScope The requested scope is invalid, unknown, malformed, or exceeds the scope granted by the resource owner.
        /// </summary>
        public const int InvalidScope = 10104;

        /// <summary>
        /// InvalidRequest The request is missing a required parameter, includes an
        /// unsupported parameter value (other than grant type),repeats a parameter, includes multiple credentials,
        /// utilizes more than one mechanism for authenticating the client, or is otherwise malformed.
        /// </summary>
        public const int InvalidRequest = 10105;

        /// <summary>
        /// InvalidGrant The provided authorization grant (e.g., authorization
        /// code, resource owner credentials) or refresh token is invalid, expired, revoked, does not match the redirection
        /// URI used in the authorization request, or was issued to another client.
        /// </summary>
        public const int InvalidGrant = 10106;

        /// <summary>
        /// The refresh token is missing. This can occur when the client does not have OfflineAccessScope for requesting a refresh token.
        /// </summary>
        public const int MissingRefreshToken = 10107;

        /// <summary>
        /// UnauthorizedClient The authenticated client is not authorized to use this authorization grant type.
        /// </summary>
        public const int UnauthorizedClient = 10108;

        /// <summary>
        /// UnsupportedGrantType The authorization grant type is not supported by the authorization server.
        /// </summary>
        public const int UnsupportedGrantType = 10109;

        /// <summary>
        /// UnsupportedResponseType The authorization server does not support obtaining an authorization code using this method.
        /// </summary>
        public const int UnsupportedResponseType = 10110;
    }
}
