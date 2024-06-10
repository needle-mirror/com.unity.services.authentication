using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Unity.Services.Authentication.Generated;
using Unity.Services.Core;
using Unity.Services.Core.Environments.Internal;
using Unity.Services.Core.Scheduler.Internal;
using Task = System.Threading.Tasks.Task;

namespace Unity.Services.Authentication
{
    partial class AuthenticationServiceInternal : IAuthenticationService
    {
        const string k_ProfileRegex = @"^[a-zA-Z0-9_-]{1,30}$";

        public event Action<RequestFailedException> SignInFailed;
        public event Action SignedIn;
        public event Action SignedOut;
        public event Action Expired;
        public event Action<SignInCodeInfo> SignInCodeReceived;
        public event Action SignInCodeExpired;
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
        public PlayerInfo PlayerInfo { get; internal set; }

        [CanBeNull]
        public string LastNotificationDate { get; private set; }

        internal long? ExpirationActionId { get; set; }
        internal long? RefreshActionId { get; set; }

        internal AccessTokenComponent AccessTokenComponent { get; }
        internal EnvironmentIdComponent EnvironmentIdComponent { get; }
        internal PlayerIdComponent PlayerIdComponent { get; }
        internal PlayerNameComponent PlayerNameComponent { get; }
        internal SessionTokenComponent SessionTokenComponent { get; }
        internal IEnvironments EnvironmentComponent { get; }
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
            SessionTokenComponent sessionToken,
            IEnvironments environment)
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
            EnvironmentComponent = environment;

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
            m_Notifications = null;

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
                    PlayerIdComponent.Refresh();
                    SessionTokenComponent.Refresh();
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
            CompleteSignIn(response.IdToken, response.SessionToken, enableRefresh, response.User, response.LastNotificationDate);
        }

        void CompleteSignIn(string accessToken, string sessionToken, bool enableRefresh = true, User user = null, string lastNotificationDate = null)
        {
            try
            {
                var accessTokenDecoded = m_JwtDecoder.Decode<AccessToken>(accessToken);
                if (accessTokenDecoded == null)
                {
                    throw AuthenticationException.Create(CommonErrorCodes.InvalidToken, "Failed to decode and verify access token.");
                }

                AccessTokenComponent.AccessToken = accessToken;

                if (accessTokenDecoded.Audience != null)
                {
                    EnvironmentIdComponent.EnvironmentId = accessTokenDecoded.Audience.FirstOrDefault(s => s.StartsWith("envId:"))?.Substring(6);
                }

                PlayerInfo = user != null ? new PlayerInfo(user) : new PlayerInfo(accessTokenDecoded.Subject);

                PlayerIdComponent.PlayerId = accessTokenDecoded.Subject;
                SessionTokenComponent.SessionToken = sessionToken;

                var expiresIn = accessTokenDecoded.Expiration - accessTokenDecoded.IssuedAt;
                var refreshTime = expiresIn - Settings.AccessTokenRefreshBuffer;
                var expiryTime = expiresIn - Settings.AccessTokenExpiryBuffer;

                if (enableRefresh && sessionToken != null && refreshTime > 0 && refreshTime < expiryTime)
                {
                    ScheduleRefresh(refreshTime);
                }

                if (expiryTime > 0)
                {
                    ScheduleExpiration(expiryTime);
                }
                LastNotificationDate = lastNotificationDate;

                ChangeState(AuthenticationState.Authorized);
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
            if (delay >= 0)
            {
                CancelScheduledRefresh();
                Logger.LogVerbose($"Scheduling refresh in {delay} seconds.");
                RefreshActionId = m_Scheduler.ScheduleAction(ExecuteScheduledRefresh, delay);
                AccessTokenComponent.RefreshTime = DateTime.UtcNow.AddSeconds(delay);
            }
            else
            {
                Logger.LogError($"Schedule delay for refresh is invalid ({delay}).");
            }
        }

        internal void ScheduleExpiration(double delay)
        {
            if (delay >= 0)
            {
                CancelScheduledExpiration();
                Logger.LogVerbose($"Scheduling expiration in {delay} seconds.");
                ExpirationActionId = m_Scheduler.ScheduleAction(ExecuteScheduledExpiration, delay);
                AccessTokenComponent.ExpiryTime = DateTime.UtcNow.AddSeconds(delay);
            }
            else
            {
                Logger.LogError($"Schedule delay for expiration is invalid ({delay}).");
            }
        }

        internal void ExecuteScheduledRefresh()
        {
            Logger.LogVerbose($"Executing scheduled refresh.");
            RefreshActionId = null;
            AccessTokenComponent.RefreshTime = null;
            RefreshAccessTokenAsync();
        }

        internal void ExecuteScheduledExpiration()
        {
            Logger.LogVerbose($"Executing scheduled expiration.");
            ExpirationActionId = null;
            AccessTokenComponent.ExpiryTime = null;
            Expire();
        }

        internal void CancelScheduledRefresh()
        {
            if (RefreshActionId.HasValue)
            {
                m_Scheduler.CancelAction(RefreshActionId.Value);
                RefreshActionId = null;
                AccessTokenComponent.RefreshTime = null;
            }
        }

        internal void CancelScheduledExpiration()
        {
            if (ExpirationActionId.HasValue)
            {
                m_Scheduler.CancelAction(ExpirationActionId.Value);
                ExpirationActionId = null;
                AccessTokenComponent.ExpiryTime = null;
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
    }
}
