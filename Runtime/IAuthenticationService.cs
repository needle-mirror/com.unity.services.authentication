using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication.Generated;
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
        /// Invoked when the SignInWithCodeAsync method successfully generates a SignInCode.
        /// </summary>
        event Action<SignInCodeInfo> SignInCodeReceived;

        /// <summary>
        /// Invoked when the sign-in code expires.
        /// </summary>
        event Action SignInCodeExpired;

        /// <summary>
        /// Invoked when a sign-in attempt has failed. The reason for failure is passed as the parameter
        /// <see cref="RequestFailedException"/>
        /// <see cref="AuthenticationException"/>.
        /// </summary>
        event Action<RequestFailedException> SignInFailed;

        /// <summary>
        /// Invoked when the name of the player changes. The new name is passed as the parameter.
        /// </summary>
        event Action<string> PlayerNameChanged;

        /// <summary>
        /// Invoked when the ID of the player changes. The new ID is passed as the parameter.
        /// </summary>
        event Action<string> PlayerIdChanged;

        /// <summary>
        /// Invoked when the PlayerInfo is set. The new PlayerInfo is passed as the parameter.
        /// </summary>
        event Action<PlayerInfo> PlayerInfoChanged;

        /// <summary>
        /// Checks whether the player is signed in or not.
        /// A player can remain signed in but have an expired session.
        /// </summary>
        bool IsSignedIn { get; }

        /// <summary>
        /// Checks whether the player is still authorized.
        /// A player is authorized as long as his access token remains valid.
        /// </summary>
        bool IsAuthorized { get; }

        /// <summary>
        /// Checks whether the player session is expired.
        /// </summary>
        bool IsExpired { get; }

        /// <summary>
        /// Returns the current player's access token when they are signed in, otherwise null.
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        /// Returns the current player's ID. This value is cached between sessions.
        /// </summary>
        string PlayerId { get; }

        /// <summary>
        /// Returns the current player's name. This value is cached between sessions.
        /// </summary>
        string PlayerName { get; }

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
        /// The date the last notification for the player was created or null if there are no notifications
        /// </summary>
        string LastNotificationDate { get; }

        /// <summary>
        /// Returns the current player's info, including linked identities.
        /// </summary>
        PlayerInfo PlayerInfo { get; }

        /// <summary>
        /// Returns player's notifications after GetNotificationsAsync is called successfully.
        /// </summary>
        List<Notification> Notifications { get; }

        /// <summary>
        /// Signs in the current player anonymously. No credentials are required and the session is confined to the current device.
        /// </summary>
        /// <remarks>
        /// If a player has signed in previously with a session token stored on the device, they are signed back in regardless of if they're an anonymous player or not.
        /// </remarks>
        /// <param name="options">Options for the operation</param>
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
        Task SignInAnonymouslyAsync(SignInOptions options = null);

        /// <summary>
        /// Sign in using Apple's ID token.
        /// If no options are used, this will create an account if none exist.
        /// </summary>
        /// <param name="idToken">Apple's ID token</param>
        /// <param name="options">Options for the operation</param>
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
        Task SignInWithAppleAsync(string idToken, SignInOptions options = null);

        /// <summary>
        /// Link the current player with the Apple account using Apple's ID token.
        /// </summary>
        /// <param name="idToken">Apple's ID token</param>
        /// <param name="options">Options for the link operations.</param>
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
        Task LinkWithAppleAsync(string idToken, LinkOptions options = null);

        /// <summary>
        /// Unlinks the Apple account from the current player account.
        /// </summary>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player has not authorized to perform this operation.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound"/> if the player's PlayerInfo does not have a matching external id.</description></item>
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
        /// Sign in using AppleGameCenter's teamPlayerId.
        /// If no options are used, this will create an account if none exist.
        /// </summary>
        /// <param name="signature">AppleGameCenter's signature</param>
        /// <param name="teamPlayerId">AppleGameCenter's teamPlayerId</param>
        /// <param name="publicKeyURL">AppleGameCenter's publicKeyURL</param>
        /// <param name="salt">AppleGameCenter's salt</param>
        /// <param name="timestamp">AppleGameCenter's timestamp</param>
        /// <param name="options">Options for the operation</param>
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
        Task SignInWithAppleGameCenterAsync(string signature, string teamPlayerId, string publicKeyURL, string salt, ulong timestamp, SignInOptions options = null);

        /// <summary>
        /// Link the current player with the AppleGameCenter account using AppleGameCenter's teamPlayerId.
        /// </summary>
        /// <param name="signature">AppleGameCenter's signature</param>
        /// <param name="teamPlayerId">AppleGameCenter's teamPlayerId</param>
        /// <param name="publicKeyURL">AppleGameCenter's publicKeyURL</param>
        /// <param name="salt">AppleGameCenter's salt</param>
        /// <param name="timestamp">AppleGameCenter's timestamp</param>
        /// <param name="options">Options for the link operations.</param>
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
        Task LinkWithAppleGameCenterAsync(string signature, string teamPlayerId, string publicKeyURL, string salt, ulong timestamp, LinkOptions options = null);

        /// <summary>
        /// Unlinks the AppleGameCenter account from the current player account.
        /// </summary>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player has not authorized to perform this operation.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound"/> if the player's PlayerInfo does not have a matching external id.</description></item>
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
        Task UnlinkAppleGameCenterAsync();

        /// <summary>
        /// Sign in using Google's ID token.
        /// If no options are used, this will create an account if none exist.
        /// </summary>
        /// <param name="idToken">Google's ID token</param>
        /// <param name="options">Options for the operation</param>
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
        Task SignInWithGoogleAsync(string idToken, SignInOptions options = null);

        /// <summary>
        /// Link the current player with the Google account using Google's ID token.
        /// </summary>
        /// <param name="idToken">Google's ID token</param>
        /// <param name="options">Options for the link operations.</param>
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
        Task LinkWithGoogleAsync(string idToken, LinkOptions options = null);

        /// <summary>
        /// Unlinks the Google account from the current player account.
        /// </summary>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player has not authorized to perform this operation.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound"/> if the player's PlayerInfo does not have a matching external id.</description></item>
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
        /// Sign in using Google Play Games' authorization code.
        /// If no options are used, this will create an account if none exist.
        /// </summary>
        /// <param name="authCode">Google Play Games' authorization code</param>
        /// <param name="options">Options for the operation</param>
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
        Task SignInWithGooglePlayGamesAsync(string authCode, SignInOptions options = null);

        /// <summary>
        /// Link the current player with the Google play games account using Google play games' authorization code.
        /// </summary>
        /// <param name="authCode">Google play games' authorization code</param>
        /// <param name="options">Options for the link operations.</param>
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
        Task LinkWithGooglePlayGamesAsync(string authCode, LinkOptions options = null);

        /// <summary>
        /// Unlinks the Google play games account from the current player account.
        /// </summary>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player has not authorized to perform this operation.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound"/> if the player's PlayerInfo does not have a matching external id.</description></item>
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
        Task UnlinkGooglePlayGamesAsync();

        /// <summary>
        /// Sign in using Facebook's access token.
        /// If no options are used, this will create an account if none exist.
        /// </summary>
        /// <param name="accessToken">Facebook's access token</param>
        /// <param name="options">Options for the operation</param>
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
        Task SignInWithFacebookAsync(string accessToken, SignInOptions options = null);

        /// <summary>
        /// Link the current player with the Facebook account using Facebook's access token.
        /// </summary>
        /// <param name="accessToken">Facebook's access token</param>
        /// <param name="options">Options for the link operations.</param>
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
        Task LinkWithFacebookAsync(string accessToken, LinkOptions options = null);

        /// <summary>
        /// Unlinks the Facebook account from the current player account.
        /// </summary>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player has not authorized to perform this operation.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound"/> if the player's PlayerInfo does not have a matching external id.</description></item>
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
        /// If no options are used, this will create an account if none exist.
        /// </summary>
        /// <param name="sessionTicket">Steam's session ticket</param>
        /// <param name="identity">The identity of the calling service</param>
        /// <param name="options">Options for the operation</param>
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
        Task SignInWithSteamAsync(string sessionTicket, string identity, SignInOptions options = null);

        /// <summary>
        /// Sign in using Steam's session ticket.
        /// If no options are used, this will create an account if none exist.
        /// This method is deprecated and may be removed in future versions.
        /// </summary>
        /// <param name="sessionTicket">Steam's session ticket</param>
        /// <param name="options">Options for the operation</param>
        /// <returns>Task for the operation</returns>
        [Obsolete("This method is deprecated as of version 2.7.1. Please use the SignInWithSteamAsync method with the 'identity' parameter for better security.")]
        Task SignInWithSteamAsync(string sessionTicket, SignInOptions options = null);

        /// <summary>
        /// Link the current player with the Steam account using Steam's session ticket.
        /// </summary>
        /// <param name="sessionTicket">Steam's session ticket</param>
        /// <param name="identity">The identity of the calling service</param>
        /// <param name="options">Options for the link operations.</param>
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
        Task LinkWithSteamAsync(string sessionTicket, string identity, LinkOptions options = null);

        /// <summary>
        /// Link the current player with the Steam account using Steam's session ticket.
        /// This method is deprecated and may be removed in future versions.
        /// </summary>
        /// <param name="sessionTicket">Steam's session ticket</param>
        /// <param name="options">Options for the link operations.</param>
        /// <returns>Task for the operation</returns>
        [Obsolete("This method is deprecated as of version 2.7.1. Please use the LinkWithSteamAsync method with the 'identity' parameter for better security.")]
        Task LinkWithSteamAsync(string sessionTicket, LinkOptions options = null);

        /// <summary>
        /// Sign in using Steam's session ticket.
        /// If no options are used, this will create an account if none exist.
        /// </summary>
        /// <param name="sessionTicket">Steam's session ticket</param>
        /// <param name="identity">The identity of the calling service</param>
        /// <param name="appId">App Id that was used to generate the ticket. Only required for additional app ids (e.g.: PlayTest, Demo, etc)</param>
        /// <param name="options">Options for the operation</param>
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
        public Task SignInWithSteamAsync(string sessionTicket, string identity, string appId, SignInOptions options = null);

        /// <summary>
        /// Link the current player with the Steam account using Steam's session ticket.
        /// </summary>
        /// <param name="sessionTicket">Steam's session ticket</param>
        /// <param name="identity">The identity of the calling service</param>
        /// <param name="appId">App Id that was used to generate the ticket. Only required for additional app ids (e.g.: PlayTest, Demo, etc)</param>
        /// <param name="options">Options for the link operations.</param>
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
        public Task LinkWithSteamAsync(string sessionTicket, string identity, string appId , LinkOptions options = null);

        /// <summary>
        /// Unlinks the Steam account from the current player account.
        /// </summary>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player has not authorized to perform this operation.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound"/> if the player's PlayerInfo does not have a matching external id.</description></item>
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
        /// Sign in using an Oculus account userId and nonce key
        /// If no options are used, this will create an account if none exists
        /// </summary>
        /// <param name="nonce">Client provided nonce key used by the server to verify that the provided Oculus userId is valid</param>
        /// <param name="userId">Oculus account userId</param>
        /// <param name="options">Options for the operation</param>
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
        Task SignInWithOculusAsync(string nonce, string userId, SignInOptions options = null);

        /// <summary>
        /// Link the current player with an Oculus account
        /// </summary>
        /// <param name="nonce">Client provided nonce key used by the server to verify that the provided Oculus userId is valid</param>
        /// <param name="userId">Oculus account userId</param>
        /// <param name="options">Options for th operation</param>
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
        Task LinkWithOculusAsync(string nonce, string userId, LinkOptions options = null);

        /// <summary>
        /// Unlinks the Oculus account from the current player account
        /// </summary>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player has not authorized to perform this operation.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound"/> if the player's PlayerInfo does not have a matching external id.</description></item>
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
        Task UnlinkOculusAsync();

        /// <summary>
        /// Sign in using a custom openID Connect id provider account.
        /// If no options are used, this will create an account if none exist.
        /// </summary>
        /// <param name="idProviderName">the name of the id provider created. Note that it must start with <i><b>&quot;oidc-&quot;</b></i> and have between 1 and 20 characters</param>
        /// <param name="idToken">Id Token for the custom id provider</param>
        /// <param name="options">Options for the operation</param>
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
        Task SignInWithOpenIdConnectAsync(string idProviderName, string idToken, SignInOptions options = null);

        /// <summary>
        /// Link the current player with a custom openID Connect id provider account.
        /// </summary>
        /// <param name="idProviderName">the name of the id provider created. Note that it must start with <i><b>&quot;oidc-&quot;</b></i> and have between 1 and 20 characters</param>
        /// <param name="idToken">Id Token for the custom id provider</param>
        /// <param name="options">Options for the operation</param>
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
        Task LinkWithOpenIdConnectAsync(string idProviderName, string idToken, LinkOptions options = null);

        /// <summary>
        /// Unlinks the custom openID Connect id provider account from the current player account.
        /// </summary>
        /// <param name="idProviderName">the name of the id provider created. Note that it must start with <i><b>&quot;oidc-&quot;</b></i> and have between 1 and 20 characters</param>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player has not authorized to perform this operation.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound"/> if the player's PlayerInfo does not have a matching external id.</description></item>
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
        Task UnlinkOpenIdConnectAsync(string idProviderName);

        /// <summary>
        /// Sign in using a Unity Player Accounts access token. The token needs to be retrieved using the PlayerAccountService API.
        /// </summary>
        /// <param name="token">Unity Player Accounts access token</param>
        /// <param name="options">Options for the operation</param>
        /// <returns>Task for the async operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.InvalidParameters"/> if parameter is empty or invalid. </description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player has already signed in or sign-in in progress.</description></item>
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
        Task SignInWithUnityAsync(string token, SignInOptions options = null);

        /// <summary>
        /// Link the current player with a Unity Player Accounts account using the Unity Player Accounts access token. The token needs to be retrieved using the PlayerAccountService API.
        /// </summary>
        /// <param name="token">Unity Player Accounts access token</param>
        /// <param name="options">Options for the link operations</param>
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
        Task LinkWithUnityAsync(string token, LinkOptions options = null);

        /// <summary>
        /// Unlinks the Unity account from the current player account
        /// </summary>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player has not authorized to perform this operation.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound"/> if the player's PlayerInfo does not have a matching external id.</description></item>
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
        Task UnlinkUnityAsync();

        /// <summary>
        /// Sign in using Username and Password credentials.
        /// </summary>
        /// <param name="username">Username of the player. Note that it must be unique per project and contains 3-20 characters of alphanumeric and/or these special characters [. - @ _].</param>
        /// <param name="password">Password of the player. Note that it must contain 8-30 characters with at least 1 upper case, 1 lower case, 1 number, and 1 special character.</param>
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
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task SignInWithUsernamePasswordAsync(string username, string password);

        /// <summary>
        /// Sign up using Username and Password credentials.
        /// </summary>
        /// <param name="username">Username of the player. Note that it must be unique per project and contains 3-20 characters of alphanumeric and/or these special characters [. - @ _].</param>
        /// <param name="password">Password of the player. Note that it must contain 8-30 characters with at least 1 upper case, 1 lower case, 1 number, and 1 special character.</param>
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
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task SignUpWithUsernamePasswordAsync(string username, string password);

        /// <summary>
        /// Sign up with a new Username/Password and add it to the current logged in user.
        /// </summary>
        /// <param name="username">Username of the player. Note that it must be unique per project and contains 3-20 characters of alphanumeric and/or these special characters [. - @ _].</param>
        /// <param name="password">Password of the player. Note that it must contain 8-30 characters with at least 1 upper case, 1 lower case, 1 number, and 1 special character.</param>
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
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task AddUsernamePasswordAsync(string username, string password);

        /// <summary>
        /// Update Password credentials for username/password user.
        /// </summary>
        /// <param name="currentPassword">Current password of the player. Note that it must contain 8-30 characters with at least 1 upper case, 1 lower case, 1 number, and 1 special character.</param>
        /// <param name="newPassword">New password of the player. Note that it must contain 8-30 characters with at least 1 upper case, 1 lower case, 1 number, and 1 special character.</param>
        /// <returns>Task for the operation</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.InvalidParameters"/> if parameter is empty or invalid. </description></item>
        /// </list>
        /// </exception>
        /// <exception cref="RequestFailedException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task UpdatePasswordAsync(string currentPassword, string newPassword);

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
        /// Returns the info of the logged in player, which includes the player's id, creation time and linked identities.
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
        Task<PlayerInfo> GetPlayerInfoAsync();

        /// <summary>
        /// Returns the name of the logged in player if it has been set.
        /// If no name has been set, this will return null if autoGenerate is set to false.
        /// This will also cache the name locally.
        /// </summary>
        /// <param name="autoGenerate">Option auto generate a player name if none already exist. Defaults to true</param>
        /// <returns>Task for the operation with the resulting player name</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player is not authorized to perform this operation.</description></item>
        /// </list>
        /// </exception>
        /// <exception cref="RequestFailedException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task<string> GetPlayerNameAsync(bool autoGenerate = true);

        /// <summary>
        /// Updates the player name of the logged in player.
        /// </summary>
        /// <param name="name">The new name for the player. It must not contain spaces.</param>
        /// <returns>Task for the operation with the resulting player name</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player is not authorized to perform this operation.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.InvalidParameters"/> if the provided player name is invalid.</description></item>
        /// </list>
        /// </exception>
        /// <exception cref="RequestFailedException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.TransportError"/> if the API call failed due to network error. Check Unity logs for more debugging information.</description></item>
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="CommonErrorCodes.Unknown"/> if the API call failed due to unexpected response from the server. Check Unity logs for more debugging information.</description></item>
        /// </list>
        /// </exception>
        Task<string> UpdatePlayerNameAsync(string name);

        /// <summary>
        /// Asynchronously generates a sign-in code that can be used to sign in. This method should be called before attempting to sign in using the sign-in code.
        /// </summary>
        /// <param name="identifier">An optional identifier. If provided, it may influence the generated sign-in code or be used for additional checks.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous generation operation. The task result contains the generated <see cref="SignInCodeInfo"/> details:
        /// <list type="bullet">
        ///     <item>
        ///         <term>SignInCode</term>
        ///         <description>The unique code generated for signing in. This code should be used in subsequent sign-in attempts.</description>
        ///     </item>
        ///     <item>
        ///         <term>Expiration</term>
        ///         <description>The date and time when the generated sign-in code will expire and become invalid for sign-in attempts.</description>
        ///     </item>
        /// </list>
        /// </returns>
        /// <exception cref="AuthenticationException">Thrown when the current authentication state is invalid for this operation.</exception>
        public Task<SignInCodeInfo> GenerateSignInCodeAsync(string identifier = null);

        /// <summary>
        ///  Asynchronously attempts to sign in using the previously generated sign-in code. This method can optionally poll the server for sign-in confirmation.
        /// </summary>
        /// <param name="usePolling">If set to true, the method will continuously poll the server to check if the code has been confirmed, until either the device signs in successfully, the code expires, or the CancellationToken is triggered. False by default.</param>
        /// <param name="cancellationToken">A token used to cancel the ongoing operation, especially useful when polling is enabled.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="AuthenticationException">Thrown when the current authentication state is invalid for this operation.</exception>
        /// <exception cref="RequestFailedException">Thrown when there's an issue with the network request.</exception>
        public Task SignInWithCodeAsync(bool usePolling = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetches detailed information about a given sign-in code, including its associated identifier and expiration details.
        /// </summary>
        /// <param name="code">The sign-in code to fetch information for.</param>
        /// <returns>The identifier associated with the provided code.</returns>
        /// <exception cref="AuthenticationException">Thrown when the code is null or empty.</exception>
        /// <exception cref="AuthenticationException">Thrown when the current authentication state is invalid for this operation.</exception>
        /// <exception cref="RequestFailedException">Thrown when there's an issue with the network request.</exception>
        public Task<SignInCodeInfo> GetSignInCodeInfoAsync(string code);

        /// <summary>
        /// Sends a request to check the status of the provided sign-in code, such as if it's been confirmed by an authenticated user or is still pending.
        /// "Confirming" the sign-in code means the server has recognized and accepted the code as valid.
        /// </summary>
        /// <param name="code">The sign-in code to be confirmed.</param>
        /// <param name="idProvider">The ID provider (optional).</param>
        /// <param name="externalToken">The external token (optional).</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation. If the operation succeeds,
        /// the task will complete successfully (indicating a successful response from the server).
        /// Otherwise, exceptions can be thrown as described below.</returns>
        /// <exception cref="AuthenticationException">Thrown when the code is null or empty.</exception>
        /// <exception cref="AuthenticationException">Thrown when the current authentication state is invalid for this operation.</exception>
        /// <exception cref="RequestFailedException">Thrown when there's an issue with the network request.</exception>
        public Task ConfirmCodeAsync(string code, string idProvider = null, string externalToken = null);

        /// <summary>
        /// Process the accessToken issued by the Unity Authentication Service. Effectively setting the access token for
        /// other UGS services to use and extracting the playerId.
        /// If the session token is set the SDK will also process it and refresh the token at the expected intervals.
        /// </summary>
        /// <param name="accessToken">Unity Player Authentication accessToken used to access other UGS services</param>
        /// <param name="sessionToken">sessionToken used to refresh the accessToken, if null the game server needs to
        /// refresh the accessToken on its own and call this method again</param>
        /// <exception cref="AuthenticationException">Thrown when the accessToken fails to parse.</exception>
        /// <exception cref="RequestFailedException">Thrown when the current authentication state is invalid for this operation.</exception>
        public void ProcessAuthenticationTokens(string accessToken, string sessionToken = null);

        /// <summary>
        /// Sign out the current player.
        /// </summary>
        /// <param name="clearCredentials">Option to clear the session token that enables logging in to the same account</param>
        void SignOut(bool clearCredentials = false);

        /// <summary>
        /// Switch the current profile.
        /// You can use profiles to sign in to multiple accounts on a single device.
        /// A profile isolates the values saved to the PlayerPrefs.
        /// The profile may only contain alphanumeric values, `-`, `_`, and must be no longer than 30 characters.
        /// The player must be signed out for this operation to succeed.
        /// </summary>
        /// <param name="profile">The profile to switch to.</param>
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

        /// <summary>
        /// Retrieves the Notifications that were created for the signed in player
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.
        /// The task result contains the list of <see cref="Notification"/>.</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully due to Authentication specific errors.
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidUserState"/> if the player is not signed out.</description></item>
        /// </list>
        /// </exception>
        Task<List<Notification>> GetNotificationsAsync();
    }
}
