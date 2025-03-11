using System;
using Unity.Services.Core;

namespace Unity.Services.Authentication.PlayerAccounts
{
    /// <summary>
    ///  A static class responsible for handling Player Accounts related errors.
    /// </summary>
    internal static class PlayerAccountsExceptionHandler
    {
        /// <summary>
        /// Handles errors by creating an appropriate <see cref="RequestFailedException"/> based on the provided error string.
        /// </summary>
        /// <param name="error">The error string describing the error.</param>
        /// <param name="description">Optional error description. Defaults to null.</param>
        /// <param name="innerException">Optional inner exception. Defaults to null.</param>
        /// <returns></returns>
        public static PlayerAccountsException HandleError(string error, string description = null, Exception innerException = null)
        {
            switch (error)
            {
                case "invalid_scope":
                    return PlayerAccountsException.Create(PlayerAccountsErrorCodes.InvalidScope, error);
                case "invalid_state":
                    return PlayerAccountsException.Create(PlayerAccountsErrorCodes.InvalidState, error);
                case "invalid_request":
                    return PlayerAccountsException.Create(PlayerAccountsErrorCodes.InvalidRequest, error);
                case "unauthorized_client":
                    return PlayerAccountsException.Create(PlayerAccountsErrorCodes.UnauthorizedClient, error);
                case "unsupported_response_type":
                    return PlayerAccountsException.Create(PlayerAccountsErrorCodes.UnsupportedResponseType, error);
                case "invalid_client":
                    return PlayerAccountsException.Create(PlayerAccountsErrorCodes.InvalidClient, description, innerException);
                case "invalid_grant":
                    return PlayerAccountsException.Create(PlayerAccountsErrorCodes.InvalidGrant, description, innerException);
                case "unsupported_grant_type":
                    return PlayerAccountsException.Create(PlayerAccountsErrorCodes.UnsupportedGrantType, description, innerException);
                default:
                    return PlayerAccountsException.Create(PlayerAccountsErrorCodes.UnknownError, error);
            }
        }
    }
}
