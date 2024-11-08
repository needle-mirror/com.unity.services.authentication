# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [3.4.0] - 2024-11-08

### Added
- Added `PlayerAuthentication` component to manage authentication in the scene authoring.

### Changed
- Updated the minimum supported Editor version to 2021.3.

## [3.3.4] - 2024-09-18
### Fixed
- Fixed an issue where `IAuthenticationService.Instance.GetSignInCodeInfoAsync()` and `IAuthenticationService.Instance.ConfirmCodeAsync()` was not working properly on iOS devices

## [3.3.3] - 2024-06-10
### Fixed
- Fixed an issue where `IAuthenticationService.SwitchProfile()` was not working properly

## [3.3.2] - 2024-05-29
### Fixed
- Fixed missing script reference in Player Accounts sample

## [3.3.1] - 2024-04-02
### Added
- Added the Apple Privacy Manifest file (PrivacyInfo.xcprivacy)
- Added support for running different profiles when using Multiplayer Player Mode.

### Fixed
- Fixed `AuthenticationState` is in invalid state when access token is changed after sign-out or sign-in
- Fixed `NullReferenceException` in Player Accounts SDK if not properly set up and the application received a deep-link request.

## [3.3.0] - 2024-01-10
### Added
- Added `LastNotificationDate` field to inform the client of the player's notifications last created date. 
- Added `GetNotificationsAsync` method to allow the client to request the player's notifications
- Added `Notifications` field to cache the notifications after GetNotificationsAsync is called
- Added `BannedUser` error code to identify an exception when a user has been banned.

### Fixed
- Fixed authentication refresh and expiration scheduling relying on device time which could cause edge cases.

## [3.2.0] - 2023-11-16

### Changed
- Changed `Custom Token Exchange` ID provider to `Custom ID`.

### Added
- Added `GetCustomId` method to PlayerInfo.
- Added `SignInWithSteamAsync` method with `appId` parameter for Multiple App Id Support
- Added `LinkWithSteamAsync` method with `appId` parameter for Multiple App Id Support
- Added `Additional App IDs` to Steam ID Provider for Multiple App Id support

### Fixed
- Fixed serialization of player accounts settings.

## [3.1.0] - 2023-10-20
### Added
- The Unity Player Accounts functionality has been merged in this package
- Added a warning when Unity Player Accounts package is installed with this version. It can be uninstalled
- Added `ProcessAuthenticationTokens` method to enable BYOI custom id sign-in

## [3.0.0] - 2023-08-21
### Changed
- Upgrading base unity version support to 2020.3
- Updated the core SDK dependency version

### Added
- Adding event in access token component for packages to be notified of changes to the `IAccessToken` value.
- Added `GenerateSignInCodeAsync` method to generate a sign-in code that can be used to sign-in.  
- Added `SignInWithCodeAsync` method to sign in using the previously generated sign-in code. This method can optionally poll the server for sign-in confirmation.
- Added `GetSignInCodeInfoAsync` method to fetch information about a given sign-in code.
- Added `ConfirmCodeAsync` method for sending a request to confirm a provided sign-in code.
- Added `SignInCodeReceived` which is triggered when `SignInWithCodeAsync` method successfully generates a`SignInCode`.
- Added `SignInCodeExpired` which is invoked when the sign-in code expires.
- Adding server authentication service which lets developers authorize operations when running on unity's game server hosting or with service accounts.

## [2.7.2] - 2023-07-19
### Added
- Added `SignInWithSteamAsync` and `LinkWithSteamAsync` methods with `identity` parameter for better security.
- Added `SignInWithUsernamePasswordAsync`, `SignUpWithUsernamePasswordAsync`, `AddUsernamePasswordAsync`, `UpdatePasswordAsync`
### Changed
- Marked previous versions of `SignInWithSteamAsync` and `LinkWithSteamAsync` methods as obsolete.

## [2.6.1] - 2023-05-31
### Added
- Added `SignInWithUnityAsync` public API.
- Added `LinkWithUnityAsync` public API.
- Added `UnlinkUnityAsync` public API.
- Added Unity Player Accounts configuration in Editor settings.
### Fixed
- Adding Preserve attributes to api models to prevent issues with code stripping for Oculus

## [2.5.0] - 2023-05-09
### Fixed
- Properly disposing web request when using player names
- Fixing exception reporting in some cases for the SignInFailed event
- Remove time validation on client side, to prevent token expiry errors caused by the wrong date/time settings on devices. 
- Adding Preserve attributes to api models to prevent issues with code stripping for player names

## [2.5.0-pre.3] - 2023-03-01
### Added
- Added `GetPlayerNameAsync` to retrieve the current player name in the Authentication service
- Added `UpdatePlayerNameAsync` to update the current player name in the Authentication service
- Added `PlayerName` property and caching in the Authentication service
### Changed
- `PlayerId` property now properly returns null instead of an empty string by default
- Package's license to refresh legal links.

## [2.4.0] - 2022-11-29
### Added
- Added `SignInWithAppleGameCenterAsync`, `LinkWithAppleGameCenterAsync`, `UnlinkAppleGameCenterAsync`
### Changed
- Updated the core SDK dependency version

## [2.3.1] - 2022-10-17
### Changed
- Updated the core SDK dependency version

## [2.3.0] - 2022-10-12
### Added
- Added `GetGooglePlayGamesId()` to `PlayerInfo`.
- Added `SignInWithOculusAsync`, `LinkWithOculusAsync`, `UnlinkOculusAsync`
- Added Oculus ID provider to Project Settings UI to configure ID providers.
### Changed
- Updated the core SDK dependency to the latest version.

## [2.2.0] - 2022-07-28
### Added
- Added `SignInWithOpenIdConnectAsync`, `LinkWithOpenIdConnectAsync`, `UnlinkOpenIdConnectAsync`
### Changed
- Updated the core SDK dependency to the latest version.

## [2.1.1] - 2022-06-22
### Changed
- Updated the core SDK dependency to latest version.
  
## [2.1.0] - 2022-06-07
### Added
- Added `SignInWithGooglePlayGames`, `LinkWithGooglePlayGames`, `UnlinkGooglePlayGames`
### Changed
- Updated the core SDK dependency to latest version.

## [2.0.0] - 2022-04-04
Public GA release.

## [1.0.0-pre.85] - 2022-02-23
### Changed
- Restructured package file hierarchy.
- Refactored PlayerInfo's CreatedAt to be a DateTime (UTC).
- Refactored PlayerInfo's identities to use new Identity type.
- ExternalId is no longer public and is replaced by Identity.

## [1.0.0-pre.83] - 2022-02-18
### Changed
- Removed 'SignInWithExternalTokenAsync and LinkWithExternalTokenAsync apis.
- Changed the following models to internal: UnlinkRequest, SignInWithExternalTokenRequest, LinkWithExternalTokenRequest

## [1.0.0-pre.80] - 2022-02-15
### Added
-  Added helper methods to retrieve external Ids
### Changed
-  Renamed public model `UserInfo` to `PlayerInfo`
-  Renamed `GetUserInfo` to `GetPlayerInfo`
-  Removed `DomainId` property from the `PlayerInfo` model

## [1.0.0-pre.77] - 2022-02-12
### Added
- Added SignInOptions models for all sign in operations.
- Added LinkOptions model for all link operations.
### Changed
- Removed SignInWithSessionTokenAsync. This is fully handled by SignInAnonymously.
- Split ExternalTokenRequest into SignInExtenalTokenRequest and LinkWithExternalTokenRequest.

## [1.0.0-pre.73] - 2022-02-08
### Changed
- Removed namespaces Unity.Services.Authentication.Models and Unity.Services.Authentication.Utilities for simplified use.

## [1.0.0-pre.67] - 2022-02-02
### Added
- Added optional parameter on Signout to clear credentials.
- Added player id caching between sessions.
- Added optional force link parameter to linking apis (Apple, Facebook, Google, Steam).
### Changed
- Reset the session token when getting an invalid session token error.

## [1.0.0-pre.44] - 2021-12-16
### Added
- Added enableRefresh parameter to SignInWithExternalTokenAsync.

## [1.0.0-pre.32] - 2021-12-03
Added profile support. Profiles allow managing multiple accounts at the same time by isolating the session token persistence.
### Added
- Added `Profile` property to `IAuthenticationService` to access the current profile.
- Added `SwitchProfile` to `IAuthenticationService` to change the current profile.
- Added `ClientInvalidProfile` error code to `AuthenticationErrorCodes` used when entering an invalid profile name.
- Added `SetProfile` extension method to `InitializationOptions`.

## [1.0.0-pre.26] - 2021-11-26
### Added
- Added `DeleteAccountAsync` to `IAuthenticationService`.
- Added `AccountLinkLimitExceeded` and `ClientUnlinkExternalIdNotFound` error codes to `AuthenticationErrorCodes`.
- Error code `AccountLinkLimitExceeded` is used when the current player's account has reached the limit of links for the provider when using a link operation.
- Error code `ClientUnlinkExternalIdNotFound` is sent when no matching external id is found in the player's `UserInfo` when using an unlink operation.

## [1.0.0-pre.20] - 2021-11-19
### Added
- Added `UserInfo` property to `IAuthenticationService`.
- Added `UnlinkAppleAsync` function to `IAuthenticationService`.
- Added `UnlinkFacebookAsync` function to `IAuthenticationService`.
- Added `UnlinkGoogleAsync` function to `IAuthenticationService`.
- Added `UnlinkSteamAsync` function to `IAuthenticationService`.

## [1.0.0-pre.14] - 2021-11-11
### Added
- Added `IsAuthorized` property.
- Added `SessionTokenExists` property.
- Added `GetUserInfoAsync` to `IAuthenticationService`

## [1.0.0-pre.8] - 2021-11-03
### Added
- Added `IsExpired` property.
- Added `Expired` event.
- Added `ClearSessionToken` function

## [1.0.0-pre.7] - 2021-10-20
### Changed
- Updated UI Samples
- Updated the core SDK dependency to latest version.

## [1.0.0-pre.6] - 2021-10-01
### Added
- Added Samples to Package Manager
- Added `SignInWithExternalTokenAsync` and `LinkWithExternalTokenAsync` to `IAuthenticationService`.
### Changed
- Made `ExternalTokenRequest` class public.

## [1.0.0-pre.5] - 2021-08-25
### Added
- Integrate with Package Manager under the Services tab filter and comply with the standard for the UI detail screen.

## [1.0.0-pre.4] - 2021-08-05
### Changed
- Updated the `AuthenticationException` to base on `RequestFailedException`.
- Updated the core SDK dependency to latest version.

## [1.0.0-pre.3] - 2021-07-30
### Fixed
- Package structure for promotion
### Changed
- Updated the core SDK dependency to latest version.

## [1.0.0-pre.2] - 2021-07-28
### Changed
- Updated the core SDK dependency to latest version.

## [1.0.0-pre.1] - 2021-07-28
### Changed
- Updated the core SDK dependency to latest version.

## [0.7.1-preview] - 2021-07-22
### Changed
- Updated the core SDK dependency to latest version.

## [0.7.0-preview] - 2021-07-22
### Changed
- Updated the core SDK dependency to latest version.
### Added
- Add missing xmldoc for the public functions.

## [0.6.0-preview] - 2021-07-15
### Added
- Add support for Unity Environments

## [0.5.0-preview] - 2021-06-16
### Changed
- Remove `SetOAuthClient()` as the authentication flow is simplified.
- Updated the initialization code to initialize with `UnityServices.Initialize()`

## [0.4.0-preview] - 2021-06-07
### Added
- Added Project Settings UI to configure ID providers.
- Added `SignInWithSteam`, `LinkWithSteam` functions.
- Changed the public interface of the Authentication service from a static instance and static methods to a singleton instance hidden behind an interface.

### Changed
- Change the public signature of `Authentication` to return a Task, as opposed to a IAsyncOperation
- Change the public API names of `Authentication` to `Async`

## [0.3.1-preview] - 2021-04-23
### Changed
- Change the `SignInFailed` event to take `AuthenticationException` instead of a string as parameter. It can provide more information for debugging purposes.
- Fixed the `com.unity.services.core` package dependency version. 

## [0.3.0-preview] - 2021-04-21
### Added
- Added `SignInWithApple`, `LinkWithApple`, `SignInWithGoogle`, `LinkWithGoogle`, `SignInWithFacebook`, `LinkWithFacebook` functions.
- Added `SignInWithSessionToken`
- Added error codes used by the social scenarios to `AuthenticationError`.

## [0.2.3-preview] - 2021-03-23
### Changed
- Rename the package from `com.unity.services.identity` to `com.unity.services.authentication`. Renamed the internal types/methods, too.

## [0.2.2-preview] - 2021-03-15
### Added
- Core package integration

## [0.2.1-preview] - 2021-03-05

- Fixed dependency on Utilities package

## [0.2.0-preview] - 2021-03-05

- Removed requirement for OAuth client ID to be specified (automatically uses project default OAuth client)

## [0.1.0-preview] - 2021-01-18

### This is the first release of *com.unity.services.identity*.
