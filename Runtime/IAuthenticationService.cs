using System;
using System.Threading.Tasks;
using Unity.Services.Authentication.Models;
using Unity.Services.Core;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// The functions for Authentication service.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Invoked when a sign-in attempt has completed successfully.
        /// </summary>
        event Action SignedIn;

        /// <summary>
        /// Invoked when a sign-out attempt has completed successfully.
        /// </summary>
        event Action SignedOut;

        /// <summary>
        /// Invoked when a session expires.
        /// </summary>
        event Action Expired;

        /// <summary>
        /// Invoked when a sign-in attempt has failed. The reason for failure is passed as the parameter
        /// <see cref="RequestFailedException"/>
        /// <see cref="AuthenticationException"/>.
        /// </summary>
        event Action<RequestFailedException> SignInFailed;

        /// <summary>
        /// Checks whether the player is signed in or not.
        /// A player can remain signed in but have an expired session.
        /// </summary>
        /// <returns>Returns true if player is signed in, else false.</returns>
        bool IsSignedIn { get; }

        /// <summary>
        /// Checks whether the player is still authorized.
        /// A player is authorized as long as his access token remains valid.
        /// </summary>
        /// <returns>Returns true if player is authorized, else false.</returns>
        bool IsAuthorized { get; }

        /// <summary>
        /// Checks whether the player session is expired.
        /// </summary>
        /// <returns>Returns true if player's session expired.</returns>
        bool IsExpired { get; }

        /// <summary>
        /// Returns the current player's access token when they are signed in, otherwise null.
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        /// Returns the current player's ID when they are signed in, otherwise null.
        /// </summary>
        string PlayerId { get; }

        /// <summary>
        /// The profile isolates the values saved to the PlayerPrefs.
        /// You can use profiles to sign in to multiple accounts on a single device.
        /// Use the <see cref="SwitchProfile(string)"/> method to change this value.
        /// </summary>
        string Profile { get; }

        /// <summary>
        /// Check if there is an existing session token stored for the current profile.
        /// </summary>
        bool SessionTokenExists { get; }

        /// <summary>
        /// Returns the current player's user info, including linked external ids.
        /// </summary>
        UserInfo UserInfo { get; }

        /// <summary>
        /// Signs in the current player anonymously. No credentials are required and the session is confined to the current device.
        /// </summary>
        /// <remarks>
        /// If a player has signed in previously with a session token stored on the device, they are signed back in regardless of if they're an anonymous player or not.
        /// </remarks>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player has already signed in or a sign-in operation is in progress.</description></item>
        /// </list>
        /// </exception>
        /// <exception cref="RequestFailedException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.InvalidToken"/> if the server side returned an invalid access token. </description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task SignInAnonymouslyAsync();

        /// <summary>
        /// Sign in the current player with the session token stored on the device.
        /// </summary>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientNoActiveSession"/> if the player does not a persisted session token.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player has already signed in or a sign-in operation is in progress.</description></item>
        /// </list>
        /// </exception>
        /// <exception cref="RequestFailedException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.InvalidToken"/> if the server side returned an invalid access token. </description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task SignInWithSessionTokenAsync();

        /// <summary>
        /// Sign in using Apple's ID token.
        /// </summary>
        /// <param name="idToken">Apple's ID token</param>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.InvalidParameters"/> if parameter is empty or invalid. </description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player has already signed in or a sign-in operation is in progress.</description></item>
        /// </list>
        /// </exception>
        /// <exception cref="RequestFailedException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.InvalidToken"/> if the server side returned an invalid access token. </description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task SignInWithAppleAsync(string idToken);

        /// <summary>
        /// Link the current player with the Apple account using Apple's ID token.
        /// </summary>
        /// <param name="idToken">Apple's ID token</param>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.AccountAlreadyLinked"/> if the player tries to link a social account while the social account is already linked with another player.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.InvalidParameters"/> if parameter is empty or invalid. </description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player is not authorized to perform this operation.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.AccountLinkLimitExceeded"/> if the player has already reached the limit of links for this provider type.</description></item>
        /// </list>
        /// </exception>
        /// <exception cref="RequestFailedException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.InvalidToken"/> if access token is invalid/expired. The access token is refreshed before it expires. This may happen if the refresh fails, or the app is unpaused with an expired access token while the refresh hasn't finished.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task LinkWithAppleAsync(string idToken);

        /// <summary>
        /// Unlinks the Apple account from the current player account.
        /// </summary>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player has not authorized to perform this operation.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound"/> if the player's UserInfo does not have a matching external id.</description></item>
        /// </list>
        /// </exception>
        /// <exception cref="RequestFailedException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.InvalidToken"/> if access token is invalid/expired. The access token is refreshed before it expires. This may happen if the refresh fails, or the app is unpaused with an expired access token while the refresh hasn't finished.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task UnlinkAppleAsync();

        /// <summary>
        /// Sign in using Google's ID token.
        /// </summary>
        /// <param name="idToken">Google's ID token</param>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.InvalidParameters"/> if parameter is empty or invalid. </description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player has already signed in or a sign-in operation is in progress.</description></item>
        /// </list>
        /// </exception>
        /// <exception cref="RequestFailedException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.InvalidToken"/> if the server side returned an invalid access token. </description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task SignInWithGoogleAsync(string idToken);

        /// <summary>
        /// Link the current player with the Google account using Google's ID token.
        /// </summary>
        /// <param name="idToken">Google's ID token</param>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.AccountAlreadyLinked"/> if the player tries to link a social account while the social account is already linked with another player.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.InvalidParameters"/> if parameter is empty or invalid. </description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player is not authorized to perform this operation.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.AccountLinkLimitExceeded"/> if the player has already reached the limit of links for this provider type.</description></item>
        /// </list>
        /// </exception>
        /// <exception cref="RequestFailedException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.InvalidToken"/> if access token is invalid/expired. The access token is refreshed before it expires. This may happen if the refresh fails, or the app is unpaused with an expired access token while the refresh hasn't finished.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task LinkWithGoogleAsync(string idToken);

        /// <summary>
        /// Unlinks the Google account from the current player account.
        /// </summary>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player has not authorized to perform this operation.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound"/> if the player's UserInfo does not have a matching external id.</description></item>
        /// </list>
        /// </exception>
        /// <exception cref="RequestFailedException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.InvalidToken"/> if access token is invalid/expired. The access token is refreshed before it expires. This may happen if the refresh fails, or the app is unpaused with an expired access token while the refresh hasn't finished.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task UnlinkGoogleAsync();

        /// <summary>
        /// Sign in using Facebook's access token.
        /// </summary>
        /// <param name="accessToken">Facebook's access token</param>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.InvalidParameters"/> if parameter is empty or invalid. </description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player has already signed in or a sign-in operation is in progress.</description></item>
        /// </list>
        /// </exception>
        /// <exception cref="RequestFailedException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.InvalidToken"/> if the server side returned an invalid access token. </description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task SignInWithFacebookAsync(string accessToken);

        /// <summary>
        /// Link the current player with the Facebook account using Facebook's access token.
        /// </summary>
        /// <param name="accessToken">Facebook's access token</param>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.AccountAlreadyLinked"/> if the player tries to link a social account while the social account is already linked with another player.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.InvalidParameters"/> if parameter is empty or invalid. </description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player is not authorized to perform this operation.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.AccountLinkLimitExceeded"/> if the player has already reached the limit of links for this provider type.</description></item>
        /// </list>
        /// </exception>
        /// <exception cref="RequestFailedException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.InvalidToken"/> if access token is invalid/expired. The access token is refreshed before it expires. This may happen if the refresh fails, or the app is unpaused with an expired access token while the refresh hasn't finished.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task LinkWithFacebookAsync(string accessToken);

        /// <summary>
        /// Unlinks the Facebook account from the current player account.
        /// </summary>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player has not authorized to perform this operation.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound"/> if the player's UserInfo does not have a matching external id.</description></item>
        /// </list>
        /// </exception>
        /// <exception cref="RequestFailedException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.InvalidToken"/> if access token is invalid/expired. The access token is refreshed before it expires. This may happen if the refresh fails, or the app is unpaused with an expired access token while the refresh hasn't finished.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task UnlinkFacebookAsync();

        /// <summary>
        /// Sign in using Steam's session ticket.
        /// </summary>
        /// <param name="sessionTicket">Steam's session ticket</param>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.InvalidParameters"/> if parameter is empty or invalid. </description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player has already signed in or a sign-in operation is in progress.</description></item>
        /// </list>
        /// </exception>
        /// <exception cref="RequestFailedException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.InvalidToken"/> if the server side returned an invalid access token. </description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task SignInWithSteamAsync(string sessionTicket);

        /// <summary>
        /// Link the current player with the Steam account using Steam's session ticket.
        /// </summary>
        /// <param name="sessionTicket">Steam's session ticket</param>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.AccountAlreadyLinked"/> if the player tries to link a social account while the social account is already linked with another player.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.InvalidParameters"/> if parameter is empty or invalid. </description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player is not authorized to perform this operation.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.AccountLinkLimitExceeded"/> if the player has already reached the limit of links for this provider type.</description></item>
        /// </list>
        /// </exception>
        /// <exception cref="RequestFailedException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.InvalidToken"/> if access token is invalid/expired. The access token is refreshed before it expires. This may happen if the refresh fails, or the app is unpaused with an expired access token while the refresh hasn't finished.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task LinkWithSteamAsync(string sessionTicket);

        /// <summary>
        /// Unlinks the Steam account from the current player account.
        /// </summary>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player has not authorized to perform this operation.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound"/> if the player's UserInfo does not have a matching external id.</description></item>
        /// </list>
        /// </exception>
        /// <exception cref="RequestFailedException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.InvalidToken"/> if access token is invalid/expired. The access token is refreshed before it expires. This may happen if the refresh fails, or the app is unpaused with an expired access token while the refresh hasn't finished.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task UnlinkSteamAsync();

        /// <summary>
        /// SignIn the current player with the external provider.
        /// </summary>
        /// <param name="externalToken">The user token from the external provider</param>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.InvalidParameters"/> if parameter is empty or invalid. </description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player has already signed in or a sign-in operation is in progress.</description></item>
        /// </list>
        /// </exception>
        /// <exception cref="RequestFailedException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.InvalidToken"/> if the server side returned an invalid access token. </description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task SignInWithExternalTokenAsync(ExternalTokenRequest externalToken);

        /// <summary>
        /// Link the current player with the external provider.
        /// </summary>
        /// <param name="externalToken">The user token from the external provider</param>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.AccountAlreadyLinked"/> if the player tries to link a social account while the social account is already linked with another player.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.InvalidParameters"/> if parameter is empty or invalid. </description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player is not authorized to perform this operation.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.AccountLinkLimitExceeded"/> if the player has already reached the limit of links for this provider type.</description></item>
        /// </list>
        /// </exception>
        /// <exception cref="RequestFailedException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.InvalidToken"/> if access token is invalid/expired. The access token is refreshed before it expires. This may happen if the refresh fails, or the app is unpaused with an expired access token while the refresh hasn't finished.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task LinkWithExternalTokenAsync(ExternalTokenRequest externalToken);

        /// <summary>
        /// Deletes the currently signed in player permanently.
        /// </summary>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player is not authorized to perform this operation.</description></item>
        /// </list>
        /// </exception>
        /// <exception cref="RequestFailedException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.InvalidToken"/> if access token is invalid/expired. The access token is refreshed before it expires. This may happen if the refresh fails, or the app is unpaused with an expired access token while the refresh hasn't finished.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task DeleteAccountAsync();

        /// <summary>
        /// Sign out the current player.
        /// Returns the info of the logged in player, which includes id, idDomain, createdAt and externalIds properties.
        /// </summary>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player is not authorized to perform this operation.</description></item>
        /// </list>
        /// </exception>
        /// <exception cref="RequestFailedException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.InvalidToken"/> if access token is invalid/expired. The access token is refreshed before it expires. This may happen if the refresh fails, or the app is unpaused with an expired access token while the refresh hasn't finished.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task<UserInfo> GetUserInfoAsync();

        /// <summary>
        /// Sign out the current player.
        /// </summary>
        void SignOut();

        /// <summary>
        /// Switch the current profile.
        /// You can use profiles to sign in to multiple accounts on a single device.
        /// A profile isolates the values saved to the PlayerPrefs.
        /// The profile may only contain alphanumeric values, `-`, `_`, and must be no longer than 30 characters.
        /// The player must be signed out for this operation to succeed.
        /// </summary>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player is not signed out.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidProfile"/> if the profile name is invalid.</description></item>
        /// </list>
        /// </exception>
        void SwitchProfile(string profile);

        /// <summary>
        /// Deletes the session token if it exists.
        /// </summary>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player is not signed out.</description></item>
        /// </list>
        /// </exception>
        void ClearSessionToken();
    }
}
