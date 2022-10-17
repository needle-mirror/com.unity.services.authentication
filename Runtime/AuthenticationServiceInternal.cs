using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Core;
using Unity.Services.Core.Scheduler.Internal;
using UnityEngine;

namespace Unity.Services.Authentication
{
    class AuthenticationServiceInternal : IAuthenticationService
    {
        const string k_ProfileRegex = @"^[a-zA-Z0-9_-]{1,30}$";
        const string k_IdProviderNameRegex = @"^oidc-[a-z0-9-_\.]{1,15}$";
        public event Action<RequestFailedException> SignInFailed;
        public event Action SignedIn;
        public event Action SignedOut;
        public event Action Expired;

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
        public PlayerInfo PlayerInfo { get; internal set; }

        internal long? ExpirationActionId { get; set; }
        internal long? RefreshActionId { get; set; }

        internal AccessTokenComponent AccessTokenComponent { get; }
        internal EnvironmentIdComponent EnvironmentIdComponent { get; }
        internal PlayerIdComponent PlayerIdComponent { get; }
        internal SessionTokenComponent SessionTokenComponent { get; }
        internal WellKnownKeysComponent WellKnownKeysComponent { get; }

        internal AuthenticationState State { get; set; }
        internal IAuthenticationSettings Settings { get; }
        internal IAuthenticationNetworkClient NetworkClient { get; set; }

        readonly IProfile m_Profile;
        readonly IJwtDecoder m_JwtDecoder;
        readonly IAuthenticationCache m_Cache;
        readonly IActionScheduler m_Scheduler;
        readonly IDateTimeWrapper m_DateTime;
        readonly IAuthenticationMetrics m_Metrics;

        internal event Action<AuthenticationState, AuthenticationState> StateChanged;

        internal AuthenticationServiceInternal(IAuthenticationSettings settings,
                                               IAuthenticationNetworkClient networkClient,
                                               IProfile profile,
                                               IJwtDecoder jwtDecoder,
                                               IAuthenticationCache cache,
                                               IActionScheduler scheduler,
                                               IDateTimeWrapper dateTime,
                                               IAuthenticationMetrics metrics,
                                               AccessTokenComponent accessToken,
                                               EnvironmentIdComponent environmentId,
                                               PlayerIdComponent playerId,
                                               SessionTokenComponent sessionToken,
                                               WellKnownKeysComponent wellKnownKeys)
        {
            Settings = settings;
            NetworkClient = networkClient;

            m_Profile = profile;
            m_JwtDecoder = jwtDecoder;
            m_Cache = cache;
            m_Scheduler = scheduler;
            m_DateTime = dateTime;
            m_Metrics = metrics;

            AccessTokenComponent = accessToken;
            EnvironmentIdComponent = environmentId;
            PlayerIdComponent = playerId;
            SessionTokenComponent = sessionToken;
            WellKnownKeysComponent = wellKnownKeys;

            State = AuthenticationState.SignedOut;
            MigrateCache();

            Expired += () => m_Metrics.SendExpiredSessionMetric();
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
                        var exception = BuildClientSessionTokenNotExistsException();
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
                    var exception = BuildClientSessionTokenNotExistsException();
                    SendSignInFailedEvent(exception, true);
                    return Task.FromException(exception);
                }
            }
            else
            {
                var exception = BuildClientInvalidStateException();
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

        public Task SignInWithSteamAsync(string sessionTicket, SignInOptions options = null)
        {
            return SignInWithExternalTokenAsync(IdProviderKeys.Steam, new SignInWithExternalTokenRequest
            {
                IdProvider = IdProviderKeys.Steam,
                Token = sessionTicket,
                SignInOnly = !options?.CreateAccount ?? false
            });
        }

        public Task LinkWithSteamAsync(string sessionTicket, LinkOptions options = null)
        {
            return LinkWithExternalTokenAsync(IdProviderKeys.Steam, new LinkWithExternalTokenRequest
            {
                IdProvider = IdProviderKeys.Steam,
                Token = sessionTicket,
                ForceLink = options?.ForceLink ?? false
            });
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
                OculusConfig = new OculusConfig(){UserId = userId},
                SignInOnly = !options?.CreateAccount ?? false
            });
        }

        public Task LinkWithOculusAsync(string nonce, string userId, LinkOptions options = null)
        {
            return LinkWithExternalTokenAsync(IdProviderKeys.Oculus, new LinkWithOculusRequest()
            {
                IdProvider = IdProviderKeys.Oculus,
                Token = nonce,
                OculusConfig = new OculusConfig(){UserId = userId},
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
                throw BuildInvalidIdProviderNameException();
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
                throw BuildInvalidIdProviderNameException();
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
                throw BuildInvalidIdProviderNameException();
            }
            return UnlinkExternalTokenAsync(idProviderName);
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
                    throw BuildServerException(e);
                }
            }
            else
            {
                throw BuildClientInvalidStateException();
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
                    throw BuildClientInvalidProfileException();
                }
            }
            else
            {
                throw BuildClientInvalidStateException();
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
                throw BuildClientInvalidStateException();
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
                    throw BuildServerException(e);
                }
            }
            else
            {
                throw BuildClientInvalidStateException();
            }
        }

        internal async Task GetWellKnownKeysAsync()
        {
            var response = await NetworkClient.GetWellKnownKeysAsync();
            WellKnownKeysComponent.Keys = response.Keys;
        }

        internal Task SignInWithExternalTokenAsync(string idProvider, SignInWithExternalTokenRequest request, bool enableRefresh = true)
        {
            if (State == AuthenticationState.SignedOut || State == AuthenticationState.Expired)
            {
                return HandleSignInRequestAsync(() => NetworkClient.SignInWithExternalTokenAsync(idProvider, request), enableRefresh);
            }
            else
            {
                var exception = BuildClientInvalidStateException();
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
                    throw BuildServerException(e);
                }
            }
            else
            {
                throw BuildClientInvalidStateException();
            }
        }

        internal async Task UnlinkExternalTokenAsync(string idProvider)
        {
            if (IsAuthorized)
            {
                var externalId = PlayerInfo?.GetIdentityId(idProvider);

                if (externalId == null)
                {
                    throw BuildClientUnlinkExternalIdNotFoundException();
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
                    throw BuildServerException(e);
                }
            }
            else
            {
                throw BuildClientInvalidStateException();
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
                var wellKnownKeysTask = WellKnownKeysComponent.Keys == null ? GetWellKnownKeysAsync() : Task.CompletedTask;
                var signinRequestTask = signInRequest();
                await Task.WhenAll(signinRequestTask, wellKnownKeysTask);
                CompleteSignIn(await signinRequestTask, enableRefresh);
            }
            catch (RequestFailedException e)
            {
                SendSignInFailedEvent(e, true);
                throw;
            }
            catch (WebRequestException e)
            {
                var authException = BuildServerException(e);
                SendSignInFailedEvent(authException, true);
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
                Logger.LogWarning("The access token is not valid. Retry JWKS and refresh again.");

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
                var accessTokenDecoded = m_JwtDecoder.Decode<AccessToken>(response.IdToken, WellKnownKeysComponent?.Keys);
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

                    AccessTokenComponent.ExpiryTime = m_DateTime.UtcNow.AddSeconds(expiryTime);

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

            if (m_DateTime.UtcNow.AddSeconds(delay) < AccessTokenComponent.ExpiryTime)
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
                Debug.LogException(e);
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

        /// <summary>
        /// Returns an exception with <c>ClientInvalidUserState</c> error
        /// when the player is in an invalid state.
        /// </summary>
        /// <returns>The exception that represents the error.</returns>
        RequestFailedException BuildClientInvalidStateException()
        {
            var errorMessage = string.Empty;

            switch (State)
            {
                case AuthenticationState.SignedOut:
                    errorMessage = "Invalid state for this operation. The player is signed out.";
                    break;
                case AuthenticationState.SigningIn:
                    errorMessage = "Invalid state for this operation. The player is already signing in.";
                    break;
                case AuthenticationState.Authorized:
                case AuthenticationState.Refreshing:
                    errorMessage = "Invalid state for this operation. The player is already signed in.";
                    break;
                case AuthenticationState.Expired:
                    errorMessage = "Invalid state for this operation. The player session has expired.";
                    break;
            }
            m_Metrics.SendClientInvalidStateExceptionMetric();
            return AuthenticationException.Create(AuthenticationErrorCodes.ClientInvalidUserState, errorMessage);
        }

        /// <summary>
        /// Returns an exception with <c>n</c> error
        /// when trying to switch to an invalid profile.
        /// </summary>
        /// <returns>The exception that represents the error.</returns>
        RequestFailedException BuildClientInvalidProfileException()
        {
            return AuthenticationException.Create(AuthenticationErrorCodes.ClientInvalidProfile, "Invalid profile name. The profile may only contain alphanumeric values, '-', '_', and must be no longer than 30 characters.");
        }

        /// <summary>
        /// Returns an exception with <c>UnlinkExternalIdNotFound</c> error
        /// when the player is calling <c>Unlink*</c> method but there is no external id found for the provider.
        /// </summary>
        /// <returns>The exception that represents the error.</returns>
        RequestFailedException BuildClientUnlinkExternalIdNotFoundException()
        {
            m_Metrics.SendUnlinkExternalIdNotFoundExceptionMetric();
            return AuthenticationException.Create(AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound, "No external id was found to unlink from the provider. Use GetPlayerInfoAsync to load the linked external ids.");
        }

        /// <summary>
        /// Returns an exception with <c>ClientNoActiveSession</c> error
        /// when the player is calling <c>SignInAnonymously</c> methods while there is no session token stored.
        /// </summary>
        /// <returns>The exception that represents the error.</returns>
        RequestFailedException BuildClientSessionTokenNotExistsException()
        {
            // At this point, the contents of the cache are invalid, and we don't want future
            m_Metrics.SendClientSessionTokenNotExistsExceptionMetric();
            // SignInAnonymously to read the current contents of the key.
            SessionTokenComponent.Clear();
            return AuthenticationException.Create(AuthenticationErrorCodes.ClientNoActiveSession, "There is no cached session token.");
        }

        /// <summary>
        /// Convert a web request exception to an authentication or request failed exception.
        /// </summary>
        /// <param name="exception">The web request exception to convert.</param>
        /// <returns>The converted exception.</returns>
        internal RequestFailedException BuildServerException(WebRequestException exception)
        {
            Logger.LogError($"Request failed: {exception.ResponseCode}, {exception.Message}");

            if (exception.NetworkError)
            {
                m_Metrics.SendNetworkErrorMetric();
                return AuthenticationException.Create(CommonErrorCodes.TransportError, $"Network Error: {exception.Message}", exception);
            }

            try
            {
                var errorResponse = JsonConvert.DeserializeObject<AuthenticationErrorResponse>(exception.Message);

                var errorCode = MapErrorCodes(errorResponse.Title);

                if (errorCode == AuthenticationErrorCodes.InvalidSessionToken)
                {
                    SessionTokenComponent.Clear();
                    Logger.LogError($"The session token is invalid and has been cleared. The associated account is no longer accessible through this login method.");
                }

                return AuthenticationException.Create(errorCode, errorResponse.Detail, exception);
            }
            catch (JsonException e)
            {
                return AuthenticationException.Create(CommonErrorCodes.Unknown, "Failed to deserialize server response.", e);
            }
            catch (Exception)
            {
                return AuthenticationException.Create(CommonErrorCodes.Unknown, "Unknown error deserializing server response. ", exception);
            }
        }

        /// <summary>
        /// Returns an exception with <c>InvalidParameters</c> error
        /// when the open id connect id provider name is not valid
        /// </summary>
        /// <returns>The exception that represents the error.</returns>
        RequestFailedException BuildInvalidIdProviderNameException()
        {
            return AuthenticationException.Create(AuthenticationErrorCodes.InvalidParameters, "Invalid IdProviderName. The Id Provider name should start with 'oidc-' and have between 6 and 20 characters (including 'oidc-')");
        }

        int MapErrorCodes(string serverErrorTitle)
        {
            switch (serverErrorTitle)
            {
                case "ENTITY_EXISTS":
                    // This is the only reason why ENTITY_EXISTS is returned so far.
                    // Include the request/API context in case it has a different meaning in the future.
                    return AuthenticationErrorCodes.AccountAlreadyLinked;
                case "LINKED_ACCOUNT_LIMIT_EXCEEDED":
                    return AuthenticationErrorCodes.AccountLinkLimitExceeded;
                case "INVALID_PARAMETERS":
                    return AuthenticationErrorCodes.InvalidParameters;
                case "INVALID_SESSION_TOKEN":
                    return AuthenticationErrorCodes.InvalidSessionToken;
                case "PERMISSION_DENIED":
                    // This is the server side response when the third party token is invalid to sign-in or link a player.
                    // Also map to AuthenticationErrorCodes.InvalidParameters since it's basically an invalid parameter.
                    // Include the request/API context in case it has a different meaning in the future.
                    return AuthenticationErrorCodes.InvalidParameters;
                case "UNAUTHORIZED_REQUEST":
                    // This happens when either the token is invalid or the token has expired.
                    return CommonErrorCodes.InvalidToken;
            }

            return CommonErrorCodes.Unknown;
        }

        bool ValidateOpenIdConnectIdProviderName(string idProviderName)
        {
            return !string.IsNullOrEmpty(idProviderName) && Regex.Match(idProviderName, k_IdProviderNameRegex).Success;
        }
    }
}
