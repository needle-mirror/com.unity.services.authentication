using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Services.Authentication.Generated;
using Unity.Services.Authentication.Shared;
using Unity.Services.Core;
using Unity.Services.Core.Scheduler.Internal;

namespace Unity.Services.Authentication
{
    class AuthenticationServiceInternal : IAuthenticationService
    {
        const string k_ProfileRegex = @"^[a-zA-Z0-9_-]{1,30}$";
        const string k_IdProviderNameRegex = @"^oidc-[a-z0-9-_\.]{1,15}$";
        const string k_SteamIdentityRegex = @"^[a-zA-Z0-9]{5,30}$";
        public event Action<RequestFailedException> SignInFailed;
        public event Action SignedIn;
        public event Action SignedOut;
        public event Action Expired;
        public event Action<RequestFailedException> UpdatePasswordFailed;

        public bool IsSignedIn =>
            State == AuthenticationState.Authorized ||
            State == AuthenticationState.Refreshing ||
            State == AuthenticationState.Expired;

        public bool IsAuthorized =>
            State == AuthenticationState.Authorized ||
            State == AuthenticationState.Refreshing;

        public bool IsExpired => State == AuthenticationState.Expired;

        public bool SessionTokenExists => !string.IsNullOrEmpty(SessionTokenComponent.SessionToken);

        public string Profile => m_Profile.Current;
        public string AccessToken => AccessTokenComponent.AccessToken;

        public string PlayerId => PlayerIdComponent.PlayerId;
        public string PlayerName => PlayerNameComponent.PlayerName;
        public PlayerInfo PlayerInfo { get; internal set; }

        internal long? ExpirationActionId { get; set; }
        internal long? RefreshActionId { get; set; }

        internal AccessTokenComponent AccessTokenComponent { get; }
        internal EnvironmentIdComponent EnvironmentIdComponent { get; }
        internal PlayerIdComponent PlayerIdComponent { get; }
        internal PlayerNameComponent PlayerNameComponent { get; }
        internal SessionTokenComponent SessionTokenComponent { get; }
        internal AuthenticationState State { get; set; }
        internal IAuthenticationSettings Settings { get; }
        internal IAuthenticationNetworkClient NetworkClient { get; set; }
        internal IPlayerNamesApi PlayerNamesApi { get; set; }
        internal IAuthenticationExceptionHandler ExceptionHandler { get; set; }

        readonly IProfile m_Profile;
        readonly IJwtDecoder m_JwtDecoder;
        readonly IAuthenticationCache m_Cache;
        readonly IActionScheduler m_Scheduler;
        readonly IAuthenticationMetrics m_Metrics;


        internal event Action<AuthenticationState, AuthenticationState> StateChanged;

        internal AuthenticationServiceInternal(
            IAuthenticationSettings settings,
            IAuthenticationNetworkClient networkClient,
            IPlayerNamesApi playerNamesApi,
            IProfile profile,
            IJwtDecoder jwtDecoder,
            IAuthenticationCache cache,
            IActionScheduler scheduler,
            IAuthenticationMetrics metrics,
            AccessTokenComponent accessToken,
            EnvironmentIdComponent environmentId,
            PlayerIdComponent playerId,
            PlayerNameComponent playerName,
            SessionTokenComponent sessionToken)
        {
            Settings = settings;
            NetworkClient = networkClient;
            PlayerNamesApi = playerNamesApi;

            m_Profile = profile;
            m_JwtDecoder = jwtDecoder;
            m_Cache = cache;
            m_Scheduler = scheduler;
            m_Metrics = metrics;

            ExceptionHandler = new AuthenticationExceptionHandler(m_Metrics);

            AccessTokenComponent = accessToken;
            EnvironmentIdComponent = environmentId;
            PlayerIdComponent = playerId;
            PlayerNameComponent = playerName;
            SessionTokenComponent = sessionToken;

            State = AuthenticationState.SignedOut;
            MigrateCache();

            PlayerIdComponent.PlayerIdChanged += OnPlayerIdChanged;
            Expired += () => m_Metrics.SendExpiredSessionMetric();
        }

        private void OnPlayerIdChanged(string playerId)
        {
            PlayerNameComponent.Clear();
        }

        public Task SignInAnonymouslyAsync(SignInOptions options = null)
        {
            if (State == AuthenticationState.SignedOut || State == AuthenticationState.Expired)
            {
                if (SessionTokenExists)
                {
                    var sessionToken = SessionTokenComponent.SessionToken;

                    if (string.IsNullOrEmpty(sessionToken))
                    {
                        SessionTokenComponent.Clear();
                        var exception = ExceptionHandler.BuildClientSessionTokenNotExistsException();
                        SendSignInFailedEvent(exception, true);
                        return Task.FromException(exception);
                    }

                    Logger.LogVerbose("Continuing existing session with cached token.");

                    return HandleSignInRequestAsync(() => NetworkClient.SignInWithSessionTokenAsync(sessionToken));
                }

                // Default behavior is to create an account.
                if (options?.CreateAccount ?? true)
                {
                    return HandleSignInRequestAsync(NetworkClient.SignInAnonymouslyAsync);
                }
                else
                {
                    SessionTokenComponent.Clear();
                    var exception = ExceptionHandler.BuildClientSessionTokenNotExistsException();
                    SendSignInFailedEvent(exception, true);
                    return Task.FromException(exception);
                }
            }
            else
            {
                var exception = ExceptionHandler.BuildClientInvalidStateException(State);
                SendSignInFailedEvent(exception, false);
                return Task.FromException(exception);
            }
        }

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

        public Task SignInWithOpenIdConnectAsync(string idProviderName, string idToken, SignInOptions options = null)
        {
            if (!ValidateOpenIdConnectIdProviderName(idProviderName))
            {
                throw ExceptionHandler.BuildInvalidIdProviderNameException();
            }
            return SignInWithExternalTokenAsync(idProviderName, new SignInWithExternalTokenRequest
            {
                IdProvider = idProviderName,
                Token = idToken,
                SignInOnly = !options?.CreateAccount ?? false
            });
        }

        public Task LinkWithOpenIdConnectAsync(string idProviderName, string idToken, LinkOptions options = null)
        {
            if (!ValidateOpenIdConnectIdProviderName(idProviderName))
            {
                throw ExceptionHandler.BuildInvalidIdProviderNameException();
            }
            return LinkWithExternalTokenAsync(idProviderName, new LinkWithExternalTokenRequest()
            {
                IdProvider = idProviderName,
                Token = idToken,
                ForceLink = options?.ForceLink ?? false
            });
        }

        public Task UnlinkOpenIdConnectAsync(string idProviderName)
        {
            if (!ValidateOpenIdConnectIdProviderName(idProviderName))
            {
                throw ExceptionHandler.BuildInvalidIdProviderNameException();
            }
            return UnlinkExternalTokenAsync(idProviderName);
        }

        public Task SignInWithUsernamePasswordAsync(string username, string password)
        {
            return SignInWithUsernamePasswordRequestAsync(BuildUsernamePasswordRequest(username, password));
        }

        public Task SignUpWithUsernamePasswordAsync(string username, string password)
        {
            return SignUpWithUsernamePasswordRequestAsync(BuildUsernamePasswordRequest(username, password));
        }

        public Task AddUsernamePasswordAsync(string username, string password)
        {
            return AddUsernamePasswordRequestAsync(BuildUsernamePasswordRequest(username, password));
        }

        public Task UpdatePasswordAsync(string currentPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
            {
                throw ExceptionHandler.BuildInvalidCredentialsException();
            }

            return UpdatePasswordRequestAsync(new UpdatePasswordRequest
            {
                Password = currentPassword,
                NewPassword = newPassword
            });
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

        public async Task DeleteAccountAsync()
        {
            if (IsAuthorized)
            {
                try
                {
                    await NetworkClient.DeleteAccountAsync(PlayerId);
                    SignOut(true);
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

        public void SignOut(bool clearCredentials = false)
        {
            AccessTokenComponent.Clear();
            PlayerInfo = null;

            if (clearCredentials)
            {
                SessionTokenComponent.Clear();
                PlayerIdComponent.Clear();
                PlayerNameComponent.Clear();
            }

            CancelScheduledRefresh();
            CancelScheduledExpiration();
            ChangeState(AuthenticationState.SignedOut);
        }

        public void SwitchProfile(string profile)
        {
            if (State == AuthenticationState.SignedOut)
            {
                if (!string.IsNullOrEmpty(profile) && Regex.Match(profile, k_ProfileRegex).Success)
                {
                    m_Profile.Current = profile;
                }
                else
                {
                    throw ExceptionHandler.BuildClientInvalidProfileException();
                }
            }
            else
            {
                throw ExceptionHandler.BuildClientInvalidStateException(State);
            }
        }

        public void ClearSessionToken()
        {
            if (State == AuthenticationState.SignedOut)
            {
                SessionTokenComponent.Clear();
            }
            else
            {
                throw ExceptionHandler.BuildClientInvalidStateException(State);
            }
        }

        public async Task<PlayerInfo> GetPlayerInfoAsync()
        {
            if (IsAuthorized)
            {
                try
                {
                    var response = await NetworkClient.GetPlayerInfoAsync(PlayerId);
                    PlayerInfo = new PlayerInfo(response);
                    return PlayerInfo;
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

        public async Task<string> GetPlayerNameAsync()
        {
            if (IsAuthorized)
            {
                try
                {
                    PlayerNamesApi.Configuration.AccessToken = AccessTokenComponent.AccessToken;
                    var response = await PlayerNamesApi.GetNameAsync(PlayerId);
                    var player = response.Data;
                    PlayerNameComponent.PlayerName = player.Name;
                    return player.Name;
                }
                catch (ApiException e)
                {
                    if (e.Response.StatusCode == 404) // HttpStatusCode.NotFound
                    {
                        PlayerNameComponent.Clear();
                        return null;
                    }

                    throw ExceptionHandler.ConvertException(e);
                }
                catch (Exception e)
                {
                    throw ExceptionHandler.BuildUnknownException(e.Message);
                }
            }
            else
            {
                throw ExceptionHandler.BuildClientInvalidStateException(State);
            }
        }

        public async Task<string> UpdatePlayerNameAsync(string playerName)
        {
            if (IsAuthorized)
            {
                if (string.IsNullOrWhiteSpace(playerName) || playerName.Any(char.IsWhiteSpace))
                {
                    throw ExceptionHandler.BuildInvalidPlayerNameException();
                }

                try
                {
                    PlayerNamesApi.Configuration.AccessToken = AccessTokenComponent.AccessToken;
                    var response = await PlayerNamesApi.UpdateNameAsync(PlayerId, new UpdateNameRequest(playerName));
                    var playerNameResult = response.Data?.Name;

                    if (string.IsNullOrWhiteSpace(playerNameResult))
                    {
                        throw ExceptionHandler.BuildUnknownException("Invalid player name response");
                    }

                    PlayerNameComponent.PlayerName = playerNameResult;
                    return playerNameResult;
                }
                catch (ApiException e)
                {
                    throw ExceptionHandler.ConvertException(e);
                }
                catch (Exception e)
                {
                    throw ExceptionHandler.BuildUnknownException(e.Message);
                }
            }
            else
            {
                throw ExceptionHandler.BuildClientInvalidStateException(State);
            }
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

        internal Task SignInWithUsernamePasswordRequestAsync(UsernamePasswordRequest request, bool enableRefresh = true)
        {
            if (State == AuthenticationState.SignedOut || State == AuthenticationState.Expired)
            {
                return HandleSignInRequestAsync(() => NetworkClient.SignInWithUsernamePasswordAsync(request), enableRefresh);
            }

            var exception = ExceptionHandler.BuildClientInvalidStateException(State);
            SendSignInFailedEvent(exception, false);
            return Task.FromException(exception);
        }

        internal Task SignUpWithUsernamePasswordRequestAsync(UsernamePasswordRequest request, bool enableRefresh = true)
        {
            if (State == AuthenticationState.SignedOut || State == AuthenticationState.Expired)
            {
                return HandleSignInRequestAsync(() => NetworkClient.SignUpWithUsernamePasswordAsync(request), enableRefresh);
            }

            var exception = ExceptionHandler.BuildClientInvalidStateException(State);
            SendSignInFailedEvent(exception, false);
            return Task.FromException(exception);
        }

        internal async Task AddUsernamePasswordRequestAsync(UsernamePasswordRequest request)
        {
            if (IsAuthorized)
            {
                try
                {
                    var response = await NetworkClient.AddUsernamePasswordAsync(request);
                    PlayerInfo.Username = response.User?.Username;
                    return;
                }
                catch (WebRequestException e)
                {
                    throw ExceptionHandler.ConvertException(e);
                }
                catch (Exception e)
                {
                    throw ExceptionHandler.BuildUnknownException(e.Message);
                }
            }
            else
            {
                throw ExceptionHandler.BuildClientInvalidStateException(State);
            }
        }

        internal Task UpdatePasswordRequestAsync(UpdatePasswordRequest request, bool enableRefresh = true)
        {
            if (IsAuthorized)
            {
                // Player is signed in, update the credentials (sessionToken, accessToken)
                return HandleUpdatePasswordRequestAsync(() => NetworkClient.UpdatePasswordAsync(request), enableRefresh);
            }
            else
            {
                var exception = ExceptionHandler.BuildClientInvalidStateException(State);
                return Task.FromException(exception);
            }
        }

        internal Task RefreshAccessTokenAsync()
        {
            if (IsSignedIn)
            {
                if (State == AuthenticationState.Expired)
                {
                    return Task.CompletedTask;
                }

                var sessionToken = SessionTokenComponent.SessionToken;

                if (string.IsNullOrEmpty(sessionToken))
                {
                    return Task.CompletedTask;
                }

                return StartRefreshAsync(sessionToken);
            }

            return Task.CompletedTask;
        }

        internal async Task HandleSignInRequestAsync(Func<Task<SignInResponse>> signInRequest, bool enableRefresh = true)
        {
            try
            {
                ChangeState(AuthenticationState.SigningIn);
                CompleteSignIn(await signInRequest(), enableRefresh);
            }
            catch (RequestFailedException e)
            {
                SendSignInFailedEvent(e, true);
                throw;
            }
            catch (WebRequestException e)
            {
                var authException = ExceptionHandler.ConvertException(e);

                if (authException.ErrorCode == AuthenticationErrorCodes.InvalidSessionToken)
                {
                    SessionTokenComponent.Clear();
                    Logger.Log($"The session token is invalid and has been cleared. The associated account is no longer accessible through this login method.");
                }

                SendSignInFailedEvent(authException, true);
                throw authException;
            }
        }

        internal async Task HandleUpdatePasswordRequestAsync(Func<Task<SignInResponse>> updatePasswordRequest, bool enableRefresh = true)
        {
            try
            {
                CompleteSignIn(await updatePasswordRequest(), enableRefresh);
            }
            catch (RequestFailedException e)
            {
                SendUpdatePasswordFailedEvent(e, false);
                throw;
            }
            catch (WebRequestException e)
            {
                var authException = ExceptionHandler.ConvertException(e);

                if (authException.ErrorCode == AuthenticationErrorCodes.InvalidSessionToken)
                {
                    SessionTokenComponent.Clear();
                    Logger.Log($"The session token is invalid and has been cleared. The associated account is no longer accessible through this login method.");
                }

                SendUpdatePasswordFailedEvent(authException, false);
                throw authException;
            }
        }

        internal async Task StartRefreshAsync(string sessionToken)
        {
            ChangeState(AuthenticationState.Refreshing);

            try
            {
                var response = await NetworkClient.SignInWithSessionTokenAsync(sessionToken);
                CompleteSignIn(response);
            }
            catch (RequestFailedException)
            {
                // Refresh failed since we received a bad token. Retry.
                Logger.LogWarning("The access token is not valid. Retry and refresh again.");

                if (State != AuthenticationState.Expired)
                {
                    Expire();
                }
            }
            catch (WebRequestException)
            {
                if (State == AuthenticationState.Refreshing)
                {
                    Logger.LogWarning("Failed to refresh access token due to network error or internal server error, will retry later.");
                    ChangeState(AuthenticationState.Authorized);
                    ScheduleRefresh(Settings.RefreshAttemptFrequency);
                }
            }
        }

        internal void CompleteSignIn(SignInResponse response, bool enableRefresh = true)
        {
            try
            {
                var accessTokenDecoded = m_JwtDecoder.Decode<AccessToken>(response.IdToken);
                if (accessTokenDecoded == null)
                {
                    throw AuthenticationException.Create(CommonErrorCodes.InvalidToken, "Failed to decode and verify access token.");
                }
                else
                {
                    AccessTokenComponent.AccessToken = response.IdToken;

                    if (accessTokenDecoded.Audience != null)
                    {
                        EnvironmentIdComponent.EnvironmentId = accessTokenDecoded.Audience.FirstOrDefault(s => s.StartsWith("envId:"))?.Substring(6);
                    }

                    PlayerInfo = response.User != null ? new PlayerInfo(response.User) : new PlayerInfo(response.UserId);

                    PlayerIdComponent.PlayerId = response.UserId;
                    SessionTokenComponent.SessionToken = response.SessionToken;

                    var refreshTime = response.ExpiresIn - Settings.AccessTokenRefreshBuffer;
                    var expiryTime = response.ExpiresIn - Settings.AccessTokenExpiryBuffer;

                    AccessTokenComponent.ExpiryTime = DateTime.UtcNow.AddSeconds(expiryTime);

                    if (enableRefresh)
                    {
                        ScheduleRefresh(refreshTime);
                    }

                    ScheduleExpiration(expiryTime);
                    ChangeState(AuthenticationState.Authorized);
                }
            }
            catch (AuthenticationException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw AuthenticationException.Create(CommonErrorCodes.Unknown, "Unknown error completing sign-in.", e);
            }
        }

        internal void ScheduleRefresh(double delay)
        {
            CancelScheduledRefresh();

            if (DateTime.UtcNow.AddSeconds(delay) < AccessTokenComponent.ExpiryTime)
            {
                Logger.LogVerbose($"Scheduling refresh in {delay} seconds.");
                RefreshActionId = m_Scheduler.ScheduleAction(ExecuteScheduledRefresh, delay);
            }
        }

        internal void ScheduleExpiration(double delay)
        {
            Logger.LogVerbose($"Scheduling expiration in {delay} seconds.");
            CancelScheduledExpiration();
            ExpirationActionId = m_Scheduler.ScheduleAction(ExecuteScheduledExpiration, delay);
        }

        internal void ExecuteScheduledRefresh()
        {
            Logger.LogVerbose($"Executing scheduled refresh.");
            RefreshActionId = null;
            RefreshAccessTokenAsync();
        }

        internal void ExecuteScheduledExpiration()
        {
            Logger.LogVerbose($"Executing scheduled expiration.");
            ExpirationActionId = null;
            Expire();
        }

        internal void CancelScheduledRefresh()
        {
            if (RefreshActionId.HasValue)
            {
                m_Scheduler.CancelAction(RefreshActionId.Value);
                RefreshActionId = null;
            }
        }

        internal void CancelScheduledExpiration()
        {
            if (ExpirationActionId.HasValue)
            {
                m_Scheduler.CancelAction(ExpirationActionId.Value);
                ExpirationActionId = null;
            }
        }

        internal void Expire()
        {
            AccessTokenComponent.Clear();
            CancelScheduledRefresh();
            CancelScheduledExpiration();
            ChangeState(AuthenticationState.Expired);
        }

        internal void MigrateCache()
        {
            try
            {
                SessionTokenComponent.Migrate();
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
        }

        void ChangeState(AuthenticationState newState)
        {
            if (State == newState)
                return;

            Logger.LogVerbose($"Moved from state [{State}] to [{newState}]");

            var oldState = State;
            State = newState;

            HandleStateChanged(oldState, newState);
        }

        void HandleStateChanged(AuthenticationState oldState, AuthenticationState newState)
        {
            StateChanged?.Invoke(oldState, newState);
            switch (newState)
            {
                case AuthenticationState.Authorized:
                    if (oldState != AuthenticationState.Refreshing)
                    {
                        SignedIn?.Invoke();
                    }

                    break;
                case AuthenticationState.SignedOut:
                    if (oldState != AuthenticationState.SigningIn)
                    {
                        SignedOut?.Invoke();
                    }
                    break;
                case AuthenticationState.Expired:
                    Expired?.Invoke();
                    break;
            }
        }

        void SendSignInFailedEvent(RequestFailedException exception, bool forceSignOut)
        {
            SignInFailed?.Invoke(exception);
            if (forceSignOut)
            {
                SignOut();
            }
        }

        void SendUpdatePasswordFailedEvent(RequestFailedException exception, bool forceSignOut)
        {
            UpdatePasswordFailed?.Invoke(exception);
            if (forceSignOut)
            {
                SignOut();
            }
        }

        bool ValidateOpenIdConnectIdProviderName(string idProviderName)
        {
            return !string.IsNullOrEmpty(idProviderName) && Regex.Match(idProviderName, k_IdProviderNameRegex).Success;
        }

        UsernamePasswordRequest BuildUsernamePasswordRequest(string username, string password)
        {
            if (!ValidateCredentials(username, password))
            {
                throw ExceptionHandler.BuildInvalidCredentialsException();
            }

            return new UsernamePasswordRequest
            {
                Username = username,
                Password = password
            };
        }

        bool ValidateCredentials(string username, string password)
        {
            return !string.IsNullOrEmpty(username)
                && !string.IsNullOrEmpty(password);
        }
    }
}
