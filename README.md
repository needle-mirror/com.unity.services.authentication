# Unity Services Authentication SDK

This package provides services for player authenticaton, unity player accounts and server authentication for Unity Gaming Services.

## Integration

Once you have installed the Authentication package, you must link your Unity project to a Unity Cloud Project using the Services window.

You need to configure the third-party login providers that you wish to use in your project in the Services Authentication window.

All services are initialized by using `UnityServices.InitializeAsync`.

## Authentication Service

The service is exposed via the `Authentication.Instance` object in the `Unity.Services.Authentication` namespace.

Once the player has been signed in, the Authentication SDK will monitor the expiration time of their access token and attempt to refresh it automatically. No further action is required.

The player's session token is cached locally in the `PlayerPrefs`. It is used to sign in again to the same account in the future.
Clearing all `PlayerPrefs` keys will require the player to sign in again from scratch on their next session.

### Public API

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
* `AuthenticationService.Instance.SignInWithAppleGameCenterAsync(string signature, string teamPlayerId, string publicKeyURL, string salt, ulong timestamp, SignInOptions options = null)`
    * This triggers the sign-in of the player with a signature from AppleGameCenter.
    * Game developer is responsible for installing the necessary SDK and get the signature from AppleGameCenter.
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
* `AuthenticationService.Instance.SignInWithSteamAsync(string sessionTicket, string identity, SignInOptions options = null)`
  * This method initiates the sign-in process for a player with their Steam account.
  * Game developer is responsible for installing the necessary SDK, getting the session ticket from Steam and providing an identity field.
  * You can provide options to control if an account should be created.
  * By default, this method will create an account if no account is currently linked.
  * If this method is called while the player is already signed in, or in the process of signing in, it will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
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
* `AuthenticationService.Instance.LinkWithAppleGameCenterAsync(string signature, string teamPlayerId, string publicKeyURL, string salt, ulong timestamp, LinkOptions options = null)`
  * This function links the current player with a signature from AppleGameCenter. The player can later sign-in with the linked AppleGameCenter account.
  * You can provide options to force the operation in case the AppleGameCenter account is already linked to another player.
  * Game developer is responsible for installing the necessary SDK and get the signature from AppleGameCenter.
  * If you attempt to link with an account that is already linked with another player, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountAlreadyLinked`.
  * If you attempt to link when there is already a AppleGameCenter account linked to this account, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountLinkLimitExceeded`.
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
* `AuthenticationService.Instance.LinkWithSteamAsync(string sessionTicket, string identity, LinkOptions options = null)`
    * This function links the current player with a session ticket from Steam. The player can later sign-in with the linked Steam account.
    * You can provide options to force the operation in case the Steam account is already linked to another player.
    * Game developer is responsible for installing the necessary SDK, getting the session ticket from Steam and providing an identity value.
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
* `AuthenticationService.Instance.UnlinkAppleGameCenterAsync()`
  * This function attempts to unlink the AppleGameCenter account using the external id from the player's PlayerInfo.
  * Use `AuthenticationService.Instance.GetPlayerInfoAsync()` to load all of your player's external ids.
  * If you attempt to link when there is already an AppleGameCenter account linked to this account, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountLinkLimitExceeded`.
  * If you attempt to unlink without being authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
  * If you attempt to unlink without having an AppleGameCenter external id in the player's PlayerInfo, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound`.
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
* `AuthenticationService.Instance.SignUpWithUsernamePasswordAsync`
  * This triggers the sign-up of the player with username and password.
  * If you attempt to sign up while already signed in or currently signing in, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.SignInWithUsernamePasswordAsync`
  * This triggers the sign-in of the player with username and password.
  * If you attempt to sign in while already signed in or currently signing in, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.AddUsernamePasswordAsync`
  * This function creates username and password credentials to an existing account. The player can later sign-in with the recently added username/password account.
  * If you attempt to link with an account that is already linked with another player, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.AccountAlreadyLinked`.
  * If you attempt to link without being authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.UpdatePasswordAsync`
  * This function updates the password for a username/password account.
  * If you update the user password while signed out the user will still be signed out.
  * If you update the user password while signed in the user will be signed in again.
  * If you attempt to update the user password while currently signing in, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.DeleteAccountAsync()`
    * This function attempts to permanently delete the current player account.
    * If you attempt to delete without being authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.GetPlayerInfoAsync()`
  * This function returns the `PlayerId`, `CreatedAt` and `ExternalIds` properties for an authorized player
  * If you are not authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.GetPlayerNameAsync()`
  * This function returns the name for the current player. It also caches it locally.
  * If no name has been set, this will return null.
  * If you are not authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
* `AuthenticationService.Instance.UpdatePlayerNameAsync(string playerName)`
  * This function attempts to update the current player name. It also caches it locally.
  * If you are not authorized, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.ClientInvalidUserState`.
  * If the provided player name is invalid, this method will throw an `AuthenticationException` with `AuthenticationErrorCodes.InvalidParameters`.
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
  * This property exposes the ID of the player. This value is cached between sessions.
* `AuthenticationService.Instance.PlayerName`
  * This property exposes the name of the player. This value is cached between sessions.
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

## Player Account Service API

Unity Player Accounts is Unity’s comprehensive sign-in solution that supports persistence across games, devices and platforms.

The service is exposed via the `PlayerAccountService.Instance` object in the `Unity.Services.Authentication.PlayerAccounts` namespace.

### Integration

- You can manually configure the settings by going to `Services->Unity Player Accounts->Configure` in the editor.
- Your **Client ID** will automatically be configured when syncing your identity providers in the editor.

### Public API

* `PlayerAccountService.Instance.SignedIn`
  * This is an event to which you can subscribe to be notified when the sign-in process has completed successfully.
* `PlayerAccountService.Instance.SignInFailed`
  * This is an event to which you can subscribe to be notified when the sign-in process has failed for some reason.
* `PlayerAccountService.Instance.SignedOut`
  * This is an event to which you can subscribe to be notified when the sign-out process has completed.
* `PlayerAccountService.Instance.AccessToken`
    * Gets the access token that was obtained during sign-in.
* `PlayerAccountService.Instance.IdToken`
    * Gets the ID token that was obtained during sign-in.
* `PlayerAccountService.Instance.AccountPortalUrl`
    * Get the player account portal url
* `PlayerAccountService.Instance.IdTokenClaims`
    * Gets the claims from the ID token that was obtained during sign-in.
 * `PlayerAccountService.Instance.IsSignedIn`
    * Checks whether the player is signed in or not.
    * A player can remain signed in but have an expired session.
* `PlayerAccountService.Instance.StartSignInAsync(bool isSigningUp = false)`
    * Starts the sign in flow by launching the system browser to sign in the current player or sign them up.
* `PlayerAccountService.Instance.RefreshTokenAsync()`
    * Refreshes the current access token using the refresh token.
* `PlayerAccountService.Instance.SignOut()`
    * Signs out the current player and revokes the access token.

## Server Authentication Service

The server authentication service is available when either `UNITY_SERVER` or `ENABLE_UCS_SERVER` conditions are enabled.

The service is exposed via the `ServerAuthenticationService.Instance` object in the `Unity.Services.Authentication.Server` namespace.

Ensure service account credentials are secured and never get included in client applications.

### Public API

* `ServerAuthenticationService.Instance.Authorized`
  * This is an event to which you can subscribe to be notified when the sign-in process has completed successfully.
* `ServerAuthenticationService.Instance.AuthorizationFailed`
  * This is an event to which you can subscribe to be notified when the sign-in process has failed for some reason.
* `ServerAuthenticationService.Instance.Expired`
  * This is an event to which you can subscribe to be notified when the sign-in session expires. The session is normally refreshed automatically, but if a session fails to refresh until the expiration time, this event will trigger.
* `ServerAuthenticationService.Instance.SignInWithServiceAccountAsync(string apiKeyIdentifier, string apiKeySecret)`
  * Trusted sign-in using service account credentials.
  * Ensure these credentials are secured and never get included in client applications.
* `ServerAuthenticationService.Instance.SignInFromServerAsync()`
  * Retrieve a token to authorize server operations from a hosted server.
  * Must be running on a multiplay server or with the server local proxy activated.
* `ServerAuthenticationService.Instance.ClearCredentials()`
    * Clears the access token and authorization state.
* `ServerAuthenticationService.Instance.AccessToken`
  * Returns the current access token, otherwise null.
* `ServerAuthenticationService.Instance.IsAuthorized`
  * Validates that the state is authorized.
* `ServerAuthenticationService.Instance.State`
  * Returns the current server authentication state.

  #### Server Authentication State Values:
  - `Authorized` : The server is currently authorized.
  - `SigningIn` : The server is currently in the process of signing in.
  - `Expired` : The server's authentication has expired.
  - `Refreshing` : The server is currently refreshing its authentication.
  - `Unauthorized` : The server is not authorized.