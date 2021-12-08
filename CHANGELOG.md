# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.0-pre.37] - 2021-12-08

## [1.0.0-pre.36] - 2021-12-07

## [1.0.0-pre.35] - 2021-12-06

## [1.0.0-pre.34] - 2021-12-05

## [1.0.0-pre.33] - 2021-12-04

## [1.0.0-pre.32] - 2021-12-03
Adding profile support. Profiles allow managing multiple accounts at the same time by isolating the session token persistence.
- Added `Profile` property to `IAuthenticationService` to access the current profile.
- Added `SwitchProfile` to `IAuthenticationService` to change the current profile.
- Added `ClientInvalidProfile` error code to `AuthenticationErrorCodes` used when entering an invalid profile name.
- Added `SetProfile` extension method to `InitializationOptions`.

## [1.0.0-pre.31] - 2021-12-02

## [1.0.0-pre.30] - 2021-12-01

## [1.0.0-pre.29] - 2021-11-29

## [1.0.0-pre.28] - 2021-11-28

## [1.0.0-pre.27] - 2021-11-27

## [1.0.0-pre.26] - 2021-11-26
- Added `DeleteAccountAsync` to `IAuthenticationService`.
- Added `AccountLinkLimitExceeded` and `ClientUnlinkExternalIdNotFound` error codes to `AuthenticationErrorCodes`.
- Error code `AccountLinkLimitExceeded` is used when the current player's account has reached the limit of links for the provider when using a link operation.
- Error code `ClientUnlinkExternalIdNotFound` is sent when no matching external id is found in the player's `UserInfo` when using an unlink operation.

## [1.0.0-pre.25] - 2021-11-24

## [1.0.0-pre.24] - 2021-11-23

## [1.0.0-pre.23] - 2021-11-22

## [1.0.0-pre.22] - 2021-11-21

## [1.0.0-pre.21] - 2021-11-20

## [1.0.0-pre.20] - 2021-11-19
- Added `UserInfo` property to `IAuthenticationService`.
- Added `UnlinkAppleAsync` function to `IAuthenticationService`.
- Added `UnlinkFacebookAsync` function to `IAuthenticationService`.
- Added `UnlinkGoogleAsync` function to `IAuthenticationService`.
- Added `UnlinkSteamAsync` function to `IAuthenticationService`.

## [1.0.0-pre.16] - 2021-11-13

## [1.0.0-pre.15] - 2021-11-12

## [1.0.0-pre.14] - 2021-11-11
- Added `IsAuthorized` property.
- Added `SessionTokenExists` property.
- Added `GetUserInfoAsync` to `IAuthenticationService`

## [1.0.0-pre.13] - 2021-11-09

## [1.0.0-pre.12] - 2021-11-07

## [1.0.0-pre.11] - 2021-11-06

## [1.0.0-pre.10] - 2021-11-05

## [1.0.0-pre.9] - 2021-11-04

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
