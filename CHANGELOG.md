# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.0-pre.6] - 2021-10-1
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
