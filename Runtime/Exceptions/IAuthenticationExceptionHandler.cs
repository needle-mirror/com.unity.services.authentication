using Unity.Services.Authentication.Shared;
using Unity.Services.Core;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// Handler for building end-user exceptions
    /// </summary>
    interface IAuthenticationExceptionHandler
    {
        /// <summary>
        /// Returns an exception with <c>ClientInvalidUserState</c> error
        /// when the player is in an invalid state.
        /// </summary>
        /// <returns>The exception that represents the error.</returns>
        RequestFailedException BuildClientInvalidStateException(AuthenticationState state);

        /// <summary>
        /// Returns an exception with <c>n</c> error
        /// when trying to switch to an invalid profile.
        /// </summary>
        /// <returns>The exception that represents the error.</returns>
        RequestFailedException BuildClientInvalidProfileException();

        /// <summary>
        /// Returns an exception with <c>UnlinkExternalIdNotFound</c> error
        /// when the player is calling <c>Unlink*</c> method but there is no external id found for the provider.
        /// </summary>
        /// <returns>The exception that represents the error.</returns>
        RequestFailedException BuildClientUnlinkExternalIdNotFoundException();

        /// <summary>
        /// Returns an exception with <c>ClientNoActiveSession</c> error
        /// when the player is calling <c>SignInAnonymously</c> methods while there is no session token stored.
        /// </summary>
        /// <returns>The exception that represents the error.</returns>
        RequestFailedException BuildClientSessionTokenNotExistsException();

        /// <summary>
        /// Returns an exception with <c>Unknown</c> error
        /// </summary>
        /// <returns>The exception that represents the error.</returns>
        RequestFailedException BuildUnknownException(string error);

        /// <summary>
        /// Returns an exception with <c>InvalidParameters</c> error
        /// when the open id connect id provider name is not valid
        /// </summary>
        /// <returns>The exception that represents the error.</returns>
        RequestFailedException BuildInvalidIdProviderNameException();

        /// <summary>
        /// Returns an exception with <c>InvalidParameters</c> error when a provided player name is invalid.
        /// </summary>
        /// <returns>The exception that represents the error.</returns>
        RequestFailedException BuildInvalidPlayerNameException();

        /// <summary>
        /// Returns an exception with <c>InvalidParameters</c> error
        /// when the username and/or password are in an incorrect format
        /// </summary>
        /// <returns>The exception that represents the error.</returns>
        RequestFailedException BuildInvalidCredentialsException();

        /// <summary>
        /// Convert a web request exception to an authentication or request failed exception.
        /// </summary>
        /// <param name="exception">The web request exception to convert.</param>
        /// <returns>The converted exception.</returns>
        RequestFailedException ConvertException(WebRequestException exception);

        /// <summary>
        /// Convert an api exception to an authentication or request failed exception.
        /// </summary>
        /// <param name="exception">The api exception to convert.</param>
        /// <returns>The converted exception.</returns>
        RequestFailedException ConvertException(ApiException exception);
    }
}
