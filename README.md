# Unity Services Authentication SDK

This package provides a service for player authenticaton for Unity Gaming Services.

## Installation

To install this package, make sure you [enable pre-release packages](https://docs.unity3d.com/2021.1/Documentation/Manual/class-PackageManager.html#advanced_preview) in the Unity Editor's Package Manager, and then follow the [installation instructions in the Unity User Manual](https://docs.unity3d.com/Documentation/Manual/upm-ui-install.html).

## Integration

Once you have installed the Authentication package, you must link your Unity project to a Unity Cloud Project using the Services window.
You need to configure the third-party login providers that you wish to use in your project in the Services Authentication window.

The Authentication SDK automatically initializes itself on game start. You can start using the Authentication API in your code right away.
The API is exposed via the `Authentication.Instance` object in the `Unity.Services.Authentication` namespace.

Once the player has been signed in, the Authentication SDK will monitor the expiration time of their access token and attempt to refresh it automatically. No further action is required.
The player's session token is cached locally in the `PlayerPrefs`. It is used to sign in again to the same account in the future.
Clearing all `PlayerPrefs` keys will require the player to sign in again from scratch on their next session.

## Public API

### Sign-In API

* `AuthenticationService.Instance.SignedIn`
  * This is an event to which you can subscribe to be notified when the sign-in process has completed successfully.
* `AuthenticationService.Instance.SignInFailed`
  * This is an event to which you can subscribe to be notified when the sign-in process has failed for some reason.
* `AuthenticationService.Instance.SignedOut`
  * This is an event to which you can subscribe to be notified when the player has been signed out.
* `AuthenticationService.Instance.Expired`
  * This is an event to which you can subscribe to be notified when the player session expires. The session is normally refreshed automatically, but if a session fails to refresh until the expiration time, this event will trigger.
* `AuthenticationService.Instance.SignInAnonymouslyAsync(SignInOptions options = null)`
    * This triggers the anonymous sign-in processes. This will generate or use the stored session token to access the account.
    * You can provide options to control if an account should be created.
    * To create a new anonymous account, use `AuthenticationService.Instance.ClearSessionToken()` while being signed out to clear the session token before signing in.
    * If you attempt to sign in while already signed in or currently signing in, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.SignInWithAppleAsync(string idToken, SignInOptions options = null)`
    * This triggers the sign-in of the player with an ID token from Apple.
    * Game developer is responsible for installing the necessary SDK and get the token from Apple.
    * You can provide options to control if an account should be created.
    * By default, this method will create an account if no account is currently linked.
    * If you attempt to sign in while already signed in or currently signing in, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.SignInWithGoogleAsync(string idToken, SignInOptions options = null)`
    * This triggers the sign-in of the player with an ID token from Google.
    * Game developer is responsible for installing the necessary SDK and get the token from Google.
    * By default, this method will create an account if no account is currently linked.
    * If you attempt to sign in while already signed in or currently signing in, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(string authCode, SignInOptions options = null)`
  * This triggers the sign-in of the player with an authorization code from Google Play Games.
  * Game developer is responsible for installing the necessary SDK and get the authorization code from Google Play Games.
  * By default, this method will create an account if no account is currently linked.
  * If you attempt to sign in while already signed in or currently signing in, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.SignInWithFacebookAsync(string accessToken, SignInOptions options = null)`
    * This triggers the sign-in of the player with an access token from Facebook.
    * Game developer is responsible for installing the necessary SDK and get the token from Facebook.
    * You can provide options to control if an account should be created.
    * By default, this method will create an account if no account is currently linked.
    * If this method is called while already signed in or currently signing in, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
*  `AuthenticationService.Instance.SignInWithOculusAsync(string nonce, string userId, SignInOptions options = null)`
   * This triggers the sign-in of the player with an Oculus account
   * Game developer is responsible for installing the necessary SDK to get the userId and nonce key.
   * By default, this method will create an account if no account is currently linked.
   * If this method is called while already signed in or currently signing in, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.SignInWithOpenIdConnectAsync(string idProviderName, string idToken, SignInOptions = null)`
  * This triggers the sign-in of the player with a custom OpenID Connect id provider account
  * Game developer is responsible for installing the necessary SDK and get the idToken.
  * By default, this method will create an account if no account is currently linked.
  * If this method is called while already signed in or currently signing in, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.LinkWithAppleAsync(string idToken, LinkOptions options = null)`
    * This function links the current player with an ID token from Apple. The player can later sign-in with the linked Apple account.
    * You can provide options to force the operation in case the Apple account is already linked to another player.
    * Game developer is responsible for installing the necessary SDK and get the token from Apple.
    * If you attempt to link with an account that is already linked with another player, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountAlreadyLinked`.
    * If you attempt to link when there is already an apple account linked to this account, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountLinkLimitExceeded`.
    * If you attempt to link without being authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.LinkWithGoogleAsync(string idToken, LinkOptions options = null)`
    * This function links the current player with an ID token from Google. The player can later sign-in with the linked Google account.
    * You can provide options to force the operation in case the Google account is already linked to another player.
    * Game developer is responsible for installing the necessary SDK and get the token from Google.
    * If you attempt to link with an account that is already linked with another player, this method will throw an `AuthenticationException` with  `AuthenticationErrorCodes.AccountAlreadyLinked`.
    * If you attempt to link when there is already a google account linked to this account, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountLinkLimitExceeded`.
    * If you attempt to link without being authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(string authCode, LinkOptions options = null)`
  * This function links the current player with an authorization code from Google Play Games. The player can later sign-in with the linked Google Play Games account.
  * You can provide options to force the operation in case the Google Play Games account is already linked to another player.
  * Game developer is responsible for installing the necessary SDK and get the authorization code from Google Play Games.
  * If you attempt to link with an account that is already linked with another player, this method will throw an `AuthenticationException` with  `AuthenticationErrorCodes.AccountAlreadyLinked`.
  * If you attempt to link when there is already a Google Play Games account linked to this account, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountLinkLimitExceeded`.
  * If you attempt to link without being authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.LinkWithFacebookAsync(string accessToken, LinkOptions options = null)`
    * This function links the current player with an access token from Facebook. The player can later sign-in with the linked Facebook account.
    * You can provide options to force the operation in case the Facebook account is already linked to another player.
    * Game developer is responsible for installing the necessary SDK and get the token from Facebook.
    * If you attempt to link with an account that is already linked with another player, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountAlreadyLinked`.
    * If you attempt to link when there is already a facebook account linked to this account, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountLinkLimitExceeded`.
    * If you attempt to link without being authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.LinkWithSteamAsync(string accessToken, LinkOptions options = null)`
    * This function links the current player with an access token from Steam. The player can later sign-in with the linked Steam account.
    * You can provide options to force the operation in case the Steam account is already linked to another player.
    * Game developer is responsible for installing the necessary SDK and get the token from Steam.
    * If you attempt to link with an account that is already linked with another player, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountAlreadyLinked`.
    * If you attempt to link when there is already a steam account linked to this account, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountLinkLimitExceeded`.
    * If you attempt to link without being authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.LinkWithOculusAsync(string nonce, string userId, LinkOptions options = null)`
  * This function links the current player with an Oculus account. The player can later sign-in with the linked account.
  * You can provide options to force the operation in case the account is already linked to another player.
  * Game developer is responsible for installing the necessary SDK to get the userId and nonce key.
  * If you attempt to link with an account that is already linked with another player, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountAlreadyLinked`.
  * If you attempt to link when there is already an Oculus account linked to this account, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountLinkLimitExceeded`.
  * If you attempt to link without being authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.LinkWithOpenIdConnectAsync(string idProviderName, string idToken, LinkOptions options = null)`
  * This function links the current player with a custom OpenID Connect id provider account. The player can later sign-in with the linked account.
  * You can provide options to force the operation in case the account is already linked to another player.
  * Game developer is responsible for installing the necessary SDK and get the token.
  * If you attempt to link with an account that is already linked with another player, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountAlreadyLinked`.
  * If you attempt to link when there is already an OpenID Connect account linked to this account, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountLinkLimitExceeded`.
  * If you attempt to link without being authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.UnlinkWithAppleAsync()`
    * This function attempts to unlink the Apple account using the external id from the player's PlayerInfo.
    * Use `AuthenticationService.Instance.GetPlayerInfoAsync()` to load all of your player's external ids.
    * If you attempt to link when there is already an Apple account linked to this account, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountLinkLimitExceeded`.
    * If you attempt to unlink without being authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
    * If you attempt to unlink without having an Apple external id in the player's PlayerInfo, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound`.
* `AuthenticationService.Instance.UnlinkWithGoogleAsync()`
    * This function attempts to unlink the Google account using the external id from the player's PlayerInfo.
    * If you attempt to link when there is already a Google account linked to this account, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountLinkLimitExceeded`.
    * If you attempt to unlink without being authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
    * If you attempt to unlink without having a Google external id in the player's PlayerInfo, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound`.
* `AuthenticationService.Instance.UnlinkWithGooglePlayGamesAsync()`
  * This function attempts to unlink the Google Play Games account using the external id from the player's PlayerInfo.
  * If you attempt to link when there is already a Google Play Games account linked to this account, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountLinkLimitExceeded`.
  * If you attempt to unlink without being authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
  * If you attempt to unlink without having a Google Play Games external id in the player's PlayerInfo, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound`.
* `AuthenticationService.Instance.UnlinkWithFacebookAsync()`
    * This function attempts to unlink the Facebook account using the external id from the player's PlayerInfo.
    * If you attempt to link when there is already a Facebook account linked to this account, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountLinkLimitExceeded`.
    * If you attempt to unlink without being authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
    * If you attempt to unlink without having a Facebook external id in the player's PlayerInfo, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound`.
* `AuthenticationService.Instance.UnlinkWithSteamAsync()`
    * This function attempts to unlink the Steam account using the external id from the player's PlayerInfo.
    * If you attempt to link when there is already a Steam account linked to this account, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountLinkLimitExceeded`.
    * If you attempt to unlink without being authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
    * If you attempt to unlink without having a Steam external id in the player's PlayerInfo, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound`.
* `AuthenticationService.Instance.UnlinkOculusAsync()`
  * This function attempts to unlink the Oculus account using the external id from the player's PlayerInfo.
  * If you attempt to link when there is already an Oculus account linked to this account, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountLinkLimitExceeded`.
  * If you attempt to unlink without being authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
  * If you attempt to unlink without having an Oculus external id in the player's PlayerInfo, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound`.
* `AuthenticationService.Instance.UnlinkOpenIdConnectAsync(string idProviderName)`
  * This function attempts to unlink the custom OpenID Connect id provider account using the external id from the player's PlayerInfo.
  * If you attempt to link when there is already an OpenID Connect account linked to this account, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountLinkLimitExceeded`.
  * If you attempt to unlink without being authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
  * If you attempt to unlink without having an external id in the player's PlayerInfo, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound`.
* `AuthenticationService.Instance.DeleteAccountAsync()`
    * This function attempts to permanently delete the current player account.
    * If you attempt to delete without being authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.GetPlayerInfoAsync()`
  * This function returns the `PlayerId`, `CreatedAt` and `ExternalIds` properties for an authorized player
  * If you are not authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.SignOut(bool clearCredentials = false)`
    * This triggers the sign-out process, which will reset the session’s access token. By default, the session token is preserved to allow the player to sign in to the same account.
  * You can optionally clear the credentials (player id, session token) by setting the clearCredentials flag to true.
* `AuthenticationService.Instance.ClearSessionToken()`
    * Clears the session token from the local cache if it exists.
    * If you are not signed out, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.SwitchProfile(string profile)`
    * Switches the current profile.
    * A profile isolates the Session Token in the `PlayerPrefs`.
    * This allows managing multiple accounts locally.
    * The profile may only contain alphanumeric values, `-`, `_`, and must be no longer than 30 characters.
    * If you are not signed out, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
    * If the profile is invalid, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidProfile`.
* `AuthenticationService.Instance.IsSignedIn`
  * Returns true if the player is signed in.
  * The player is still considered signed in even if the access token expires, until `AuthenticationService.Instance.SignOut()` is explicitly called.
* `AuthenticationService.Instance.IsAuthorized`
  * Returns true if the player is signed in and the access token is valid.
* `AuthenticationService.Instance.IsExpired`
  * Returns true if the access token has expired.
* `AuthenticationService.Instance.PlayerId`
  * This property exposes the ID of the player when they are signed in, or null if they are not.
* `AuthenticationService.Instance.Profile`
  * This property exposes the current profile.
    * A profile isolates the Session Token in the `PlayerPrefs`.
    * This allows managing multiple accounts locally.
    * Profiles are not persisted and are optional.
* `AuthenticationService.Instance.AccessToken`
  * Returns the raw string of the current access token, or null if no valid token is available (e.g. the player is signed in but the token has expired and could not be refreshed).
  * This value is updated automatically by the refresh process, so consumers should NOT cache this value.
* `AuthenticationService.Instance.SessionTokenExists`
    * Returns True if there is a cached session token.
    * The session token is updated after each login and preserved locally on the device.
    * It can be used to authenticate the player to the same account in the future.
  * You can use `AuthenticationService.Instance.ClearSessionToken()` to clear the token from the local cache.
* `AuthenticationService.Instance.PlayerInfo`
  * Returns the current player's player info, such as account creation time and linked external ids.
    * This is only partially loaded during sign in operations.
    * Use `AuthenticationService.Instance.GetPlayerInfoAsync()` to load the all the player info.
