using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Unity.Services.Authentication
{
    partial class AuthenticationServiceInternal
    {
        const string k_SteamIdentityRegex = @"^[a-zA-Z0-9]{5,30}$";

        public Task SignInWithAppleAsync(string idToken, SignInOptions options = null)
        {
            return SignInWithExternalTokenAsync(IdProviderKeys.Apple, new SignInWithExternalTokenRequest
            {
                IdProvider = IdProviderKeys.Apple,
                Token = idToken,
                SignInOnly = !options?.CreateAccount ?? false
            });
        }

        public Task LinkWithAppleAsync(string idToken, LinkOptions options = null)
        {
            return LinkWithExternalTokenAsync(IdProviderKeys.Apple, new LinkWithExternalTokenRequest
            {
                IdProvider = IdProviderKeys.Apple,
                Token = idToken,
                ForceLink = options?.ForceLink ?? false
            });
        }

        public Task UnlinkAppleAsync()
        {
            return UnlinkExternalTokenAsync(IdProviderKeys.Apple);
        }

        public Task SignInWithAppleGameCenterAsync(string signature, string teamPlayerId, string publicKeyURL,
            string salt, ulong timestamp, SignInOptions options = null)
        {
            return SignInWithExternalTokenAsync(IdProviderKeys.AppleGameCenter, new SignInWithAppleGameCenterRequest()
            {
                IdProvider = IdProviderKeys.AppleGameCenter,
                Token = signature,
                AppleGameCenterConfig = new AppleGameCenterConfig() { TeamPlayerId = teamPlayerId, PublicKeyURL = publicKeyURL, Salt = salt, Timestamp = timestamp },
                SignInOnly = !options?.CreateAccount ?? false
            });
        }

        public Task LinkWithAppleGameCenterAsync(string signature, string teamPlayerId, string publicKeyURL,
            string salt, ulong timestamp, LinkOptions options = null)
        {
            return LinkWithExternalTokenAsync(IdProviderKeys.AppleGameCenter, new LinkWithAppleGameCenterRequest()
            {
                IdProvider = IdProviderKeys.AppleGameCenter,
                Token = signature,
                AppleGameCenterConfig = new AppleGameCenterConfig() { TeamPlayerId = teamPlayerId, PublicKeyURL = publicKeyURL, Salt = salt, Timestamp = timestamp },
                ForceLink = options?.ForceLink ?? false
            });
        }

        public Task UnlinkAppleGameCenterAsync()
        {
            return UnlinkExternalTokenAsync(IdProviderKeys.AppleGameCenter);
        }

        public Task SignInWithFacebookAsync(string accessToken, SignInOptions options = null)
        {
            return SignInWithExternalTokenAsync(IdProviderKeys.Facebook, new SignInWithExternalTokenRequest
            {
                IdProvider = IdProviderKeys.Facebook,
                Token = accessToken,
                SignInOnly = !options?.CreateAccount ?? false
            });
        }

        public Task LinkWithFacebookAsync(string accessToken, LinkOptions options = null)
        {
            return LinkWithExternalTokenAsync(IdProviderKeys.Facebook, new LinkWithExternalTokenRequest
            {
                IdProvider = IdProviderKeys.Facebook,
                Token = accessToken,
                ForceLink = options?.ForceLink ?? false
            });
        }

        public Task UnlinkFacebookAsync()
        {
            return UnlinkExternalTokenAsync(IdProviderKeys.Facebook);
        }

        public Task SignInWithGoogleAsync(string idToken, SignInOptions options = null)
        {
            return SignInWithExternalTokenAsync(IdProviderKeys.Google, new SignInWithExternalTokenRequest
            {
                IdProvider = IdProviderKeys.Google,
                Token = idToken,
                SignInOnly = !options?.CreateAccount ?? false
            });
        }

        public Task LinkWithGoogleAsync(string idToken, LinkOptions options = null)
        {
            return LinkWithExternalTokenAsync(IdProviderKeys.Google, new LinkWithExternalTokenRequest
            {
                IdProvider = IdProviderKeys.Google,
                Token = idToken,
                ForceLink = options?.ForceLink ?? false
            });
        }

        public Task UnlinkGoogleAsync()
        {
            return UnlinkExternalTokenAsync(IdProviderKeys.Google);
        }

        public Task SignInWithGooglePlayGamesAsync(string authCode, SignInOptions options = null)
        {
            return SignInWithExternalTokenAsync(IdProviderKeys.GooglePlayGames, new SignInWithExternalTokenRequest
            {
                IdProvider = IdProviderKeys.GooglePlayGames,
                Token = authCode,
                SignInOnly = !options?.CreateAccount ?? false
            });
        }

        public Task LinkWithGooglePlayGamesAsync(string authCode, LinkOptions options = null)
        {
            return LinkWithExternalTokenAsync(IdProviderKeys.GooglePlayGames, new LinkWithExternalTokenRequest
            {
                IdProvider = IdProviderKeys.GooglePlayGames,
                Token = authCode,
                ForceLink = options?.ForceLink ?? false
            });
        }

        public Task UnlinkGooglePlayGamesAsync()
        {
            return UnlinkExternalTokenAsync(IdProviderKeys.GooglePlayGames);
        }

        public Task SignInWithOculusAsync(string nonce, string userId, SignInOptions options = null)
        {
            return SignInWithExternalTokenAsync(IdProviderKeys.Oculus, new SignInWithOculusRequest
            {
                IdProvider = IdProviderKeys.Oculus,
                Token = nonce,
                OculusConfig = new OculusConfig() { UserId = userId },
                SignInOnly = !options?.CreateAccount ?? false
            });
        }

        public Task LinkWithOculusAsync(string nonce, string userId, LinkOptions options = null)
        {
            return LinkWithExternalTokenAsync(IdProviderKeys.Oculus, new LinkWithOculusRequest()
            {
                IdProvider = IdProviderKeys.Oculus,
                Token = nonce,
                OculusConfig = new OculusConfig() { UserId = userId },
                ForceLink = options?.ForceLink ?? false
            });
        }

        public Task UnlinkOculusAsync()
        {
            return UnlinkExternalTokenAsync(IdProviderKeys.Oculus);
        }

        [Obsolete("This method is deprecated as of version 2.7.1. Please use the SignInWithSteamAsync method with the 'identity' parameter for better security.")]
        public Task SignInWithSteamAsync(string sessionTicket, SignInOptions options = null)
        {
            return SignInWithExternalTokenAsync(IdProviderKeys.Steam,
                new SignInWithSteamRequest
                {
                    IdProvider = IdProviderKeys.Steam,
                    Token = sessionTicket,
                    SignInOnly = !options?.CreateAccount ?? false
                });
        }

        [Obsolete("This method is deprecated as of version 2.7.1. Please use the LinkWithSteamAsync method with the 'identity' parameter for better security.")]
        public Task LinkWithSteamAsync(string sessionTicket, LinkOptions options = null)
        {
            return LinkWithExternalTokenAsync(IdProviderKeys.Steam,
                new LinkWithSteamRequest
                {
                    IdProvider = IdProviderKeys.Steam,
                    Token = sessionTicket,
                    ForceLink = options?.ForceLink ?? false
                });
        }

        public Task SignInWithSteamAsync(string sessionTicket, string identity, SignInOptions options = null)
        {
            ValidateSteamIdentity(identity);

            return SignInWithExternalTokenAsync(IdProviderKeys.Steam,
                new SignInWithSteamRequest
                {
                    IdProvider = IdProviderKeys.Steam,
                    Token = sessionTicket,
                    SteamConfig = new SteamConfig() {identity = identity},
                    SignInOnly = !options?.CreateAccount ?? false
                });
        }

        public Task LinkWithSteamAsync(string sessionTicket, string identity, LinkOptions options = null)
        {
            ValidateSteamIdentity(identity);

            return LinkWithExternalTokenAsync(IdProviderKeys.Steam,
                new LinkWithSteamRequest
                {
                    IdProvider = IdProviderKeys.Steam,
                    Token = sessionTicket,
                    SteamConfig = new SteamConfig() {identity = identity},
                    ForceLink = options?.ForceLink ?? false
                });
        }

        public Task SignInWithSteamAsync(string sessionTicket, string identity, string appId, SignInOptions options = null)
        {
            ValidateSteamIdentity(identity);

            return SignInWithExternalTokenAsync(IdProviderKeys.Steam,
                new SignInWithSteamRequest
                {
                    IdProvider = IdProviderKeys.Steam,
                    Token = sessionTicket,
                    SteamConfig = new SteamConfig()
                    {
                        identity = identity,
                        appId = appId
                    },
                    SignInOnly = !options?.CreateAccount ?? false
                });
        }

        public Task LinkWithSteamAsync(string sessionTicket, string identity, string appId, LinkOptions options = null)
        {
            ValidateSteamIdentity(identity);

            return LinkWithExternalTokenAsync(IdProviderKeys.Steam,
                new LinkWithSteamRequest
                {
                    IdProvider = IdProviderKeys.Steam,
                    Token = sessionTicket,
                    SteamConfig = new SteamConfig()
                    {
                        identity = identity,
                        appId = appId
                    },
                    ForceLink = options?.ForceLink ?? false
                });
        }

        void ValidateSteamIdentity(string identity)
        {
            if (string.IsNullOrEmpty(identity))
            {
                throw ExceptionHandler.BuildUnknownException("Identity cannot be null or empty.");
            }

            if (!Regex.IsMatch(identity, k_SteamIdentityRegex))
            {
                throw ExceptionHandler.BuildUnknownException("The provided identity must only contain alphanumeric characters and be between 5 and 30 characters in length.");
            }
        }

        public Task UnlinkSteamAsync()
        {
            return UnlinkExternalTokenAsync(IdProviderKeys.Steam);
        }

        public Task SignInWithUnityAsync(string token, SignInOptions options = null)
        {
            return SignInWithExternalTokenAsync(IdProviderKeys.Unity, new SignInWithExternalTokenRequest
            {
                IdProvider = IdProviderKeys.Unity,
                Token = token,
                SignInOnly = !options?.CreateAccount ?? false
            });
        }

        public Task LinkWithUnityAsync(string token, LinkOptions options = null)
        {
            return LinkWithExternalTokenAsync(IdProviderKeys.Unity, new LinkWithExternalTokenRequest
            {
                IdProvider = IdProviderKeys.Unity,
                Token = token,
                ForceLink = options?.ForceLink ?? false
            });
        }

        public Task UnlinkUnityAsync()
        {
            return UnlinkExternalTokenAsync(IdProviderKeys.Unity);
        }

        internal Task SignInWithExternalTokenAsync(string idProvider, SignInWithExternalTokenRequest request, bool enableRefresh = true)
        {
            if (State == AuthenticationState.SignedOut || State == AuthenticationState.Expired)
            {
                return HandleSignInRequestAsync(() => NetworkClient.SignInWithExternalTokenAsync(idProvider, request), enableRefresh);
            }
            else
            {
                var exception = ExceptionHandler.BuildClientInvalidStateException(State);
                SendSignInFailedEvent(exception, false);
                return Task.FromException(exception);
            }
        }

        internal async Task LinkWithExternalTokenAsync(string idProvider, LinkWithExternalTokenRequest request)
        {
            if (IsAuthorized)
            {
                try
                {
                    var response = await NetworkClient.LinkWithExternalTokenAsync(idProvider, request);
                    PlayerInfo?.AddExternalIdentity(response.User?.ExternalIds?.FirstOrDefault(x => x.ProviderId == request.IdProvider));
                }
                catch (WebRequestException e)
                {
                    throw ExceptionHandler.ConvertException(e);
                }
            }
            else
            {
                throw ExceptionHandler.BuildClientInvalidStateException(State);
            }
        }

        internal async Task UnlinkExternalTokenAsync(string idProvider)
        {
            if (IsAuthorized)
            {
                var externalId = PlayerInfo?.GetIdentityId(idProvider);

                if (externalId == null)
                {
                    throw ExceptionHandler.BuildClientUnlinkExternalIdNotFoundException();
                }

                try
                {
                    await NetworkClient.UnlinkExternalTokenAsync(idProvider, new UnlinkRequest
                    {
                        IdProvider = idProvider,
                        ExternalId = externalId
                    });

                    PlayerInfo.RemoveIdentity(idProvider);
                }
                catch (WebRequestException e)
                {
                    throw ExceptionHandler.ConvertException(e);
                }
            }
            else
            {
                throw ExceptionHandler.BuildClientInvalidStateException(State);
            }
        }
    }
}
