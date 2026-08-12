using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication.Server.Environments.Generated;
using Unity.Services.Authentication.Server.Proxy.Generated;
using Unity.Services.Authentication.Server.ServiceAuth.Generated;
using Unity.Services.Authentication.Server.Shared;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Environments.Internal;
using Unity.Services.Core.Scheduler.Internal;

namespace Unity.Services.Authentication.Server
{
    class ServerAuthenticationServiceInternal : IServerAuthenticationService
    {
        public event Action Authorized;
        public event Action Expired;
        public event Action<ServerAuthenticationException> AuthorizationFailed;

        public ServerAuthenticationState State { get; internal set; }

        public bool IsAuthorized => State == ServerAuthenticationState.Authorized || State == ServerAuthenticationState.Refreshing;
        public string AccessToken => ServerAccessTokenComponent.AccessToken;

        internal ServerEnvironmentIdComponent ServerEnvironmentIdComponent { get; }
        internal ServerAccessTokenComponent ServerAccessTokenComponent { get; }

        internal long? ExpirationActionId { get; set; }
        internal long? RefreshActionId { get; set; }
        internal IServerAuthenticationSettings Settings { get; }
        internal AuthType AuthType { get; set; }

        internal event Action<ServerAuthenticationState, ServerAuthenticationState> StateChanged;

        readonly IEnvironmentApi m_EnvironmentApi;
        readonly IServiceAuthenticationApi m_ServiceAuthApi;
        readonly IProxyApi m_ProxyApi;

        readonly ICloudProjectId m_CloudProjectId;
        readonly IEnvironments m_Environment;
        readonly IActionScheduler m_Scheduler;
        readonly IDateTimeWrapper m_DateTime;
        readonly IJwtDecoder m_JwtDecoder;
        readonly IServerConfiguration m_Configuration;

        /// <summary>
        /// The scopes requested during sign-in, kept so that a refreshed token is granted the same scopes.
        /// </summary>
        List<string> m_Scopes;

        /// <summary>
        /// Identifies the current authorization session. A refresh captures it before requesting a token and
        /// discards the response if the session ended while the request was in flight.
        /// Incremented by <see cref="Reset"/> and <see cref="Expire"/>, the only two ways a session ends.
        /// </summary>
        int m_SessionGeneration;

        internal ServerAuthenticationServiceInternal(
            IServerAuthenticationSettings settings,
            ICloudProjectId cloudProjectId,
            IJwtDecoder jwtDecoder,
            IActionScheduler scheduler,
            IDateTimeWrapper dateTime,
            IEnvironments environment,
            ServerAccessTokenComponent serviceAccessTokenComponent,
            ServerEnvironmentIdComponent serverEnvironmentIdComponent,
            IServerConfiguration configuration,
            IEnvironmentApi enviromentApi,
            IServiceAuthenticationApi serviceAuthApi,
            IProxyApi ProxyApi)
        {
            Settings = settings;
            m_CloudProjectId = cloudProjectId;
            m_JwtDecoder = jwtDecoder;
            m_Scheduler = scheduler;
            m_DateTime = dateTime;
            m_Environment = environment;
            ServerAccessTokenComponent = serviceAccessTokenComponent;
            ServerEnvironmentIdComponent = serverEnvironmentIdComponent;

            m_Configuration = configuration;

            m_EnvironmentApi = enviromentApi;
            m_ServiceAuthApi = serviceAuthApi;
            m_ProxyApi = ProxyApi;

            State = ServerAuthenticationState.Unauthorized;
        }

        public Task SignInWithServiceAccountAsync(string apiKeyIdentifier, string apiKeySecret)
        {
            return SignInWithServiceAccountAsync(apiKeyIdentifier, apiKeySecret, null);
        }

        public async Task SignInWithServiceAccountAsync(string apiKeyIdentifier, string apiKeySecret, List<string> scopes)
        {
            ValidateSignInState();

            try
            {
                State = ServerAuthenticationState.SigningIn;
                AuthType = AuthType.ServiceAccount;

                var accessToken = await RequestServiceAccountTokenAsync(apiKeyIdentifier, apiKeySecret, scopes);
                ProcessResponse(accessToken);
            }
            catch (ApiException apiException)
            {
                throw HandleSignInException(ServerAuthenticationExceptionFactory.Create(apiException));
            }
            catch (Exception unknownException)
            {
                throw HandleSignInException(ServerAuthenticationExceptionFactory.Create(unknownException));
            }
        }

        public async Task SignInFromServerAsync()
        {
            ValidateSignInState();

            try
            {
                State = ServerAuthenticationState.SigningIn;
                AuthType = AuthType.Proxy;

                var accessToken = await RequestProxyTokenAsync();
                ProcessResponse(accessToken);
            }
            catch (ApiException apiException)
            {
                throw HandleSignInException(ServerAuthenticationExceptionFactory.Create(apiException));
            }
            catch (Exception unknownException)
            {
                throw HandleSignInException(ServerAuthenticationExceptionFactory.Create(unknownException));
            }
        }

        /// <summary>
        /// Throws when the service is not in a state that allows starting a new sign-in.
        /// </summary>
        void ValidateSignInState()
        {
            if (State == ServerAuthenticationState.Unauthorized || State == ServerAuthenticationState.Expired)
                return;

            var exception = ServerAuthenticationExceptionFactory.CreateClientInvalidState(State);
            OnAuthorizationFailed(exception);
            throw exception;
        }

        /// <summary>
        /// Reports a failed sign-in attempt and clears the authorization state.
        /// </summary>
        /// <param name="exception">The exception describing the failure.</param>
        /// <returns>The exception to throw to the caller.</returns>
        ServerAuthenticationException HandleSignInException(ServerAuthenticationException exception)
        {
            OnAuthorizationFailed(exception);
            Reset();
            return exception;
        }

        /// <summary>
        /// Requests a stateless access token for the service account.
        /// Used both for sign-in and for refreshing an existing token, so it does not apply the token itself:
        /// the caller decides whether the result is still wanted and how a failure is handled.
        /// </summary>
        async Task<string> RequestServiceAccountTokenAsync(string apiKeyIdentifier, string apiKeySecret, List<string> scopes)
        {
            if (string.IsNullOrEmpty(apiKeyIdentifier) || string.IsNullOrEmpty(apiKeySecret))
                throw ServerAuthenticationException.Create(ServerAuthenticationErrorCodes.InvalidParameters, $"Invalid parameters.");

            SetServiceAccount(apiKeyIdentifier, apiKeySecret);

            // Copied so that a caller mutating its own list cannot change the scopes of an active session.
            m_Scopes = scopes == null ? null : new List<string>(scopes);

            await FulfillEnvironmentIdAsync();

            var request = new ExchangeRequest(m_Scopes);
            var response = await m_ServiceAuthApi.ExchangeToStatelessAsync(m_CloudProjectId.GetCloudProjectId(), ServerEnvironmentIdComponent.EnvironmentId, request);

            return response.Data.AccessToken;
        }

        /// <summary>
        /// Requests an access token from the local server proxy.
        /// Used both for sign-in and for refreshing an existing token.
        /// </summary>
        async Task<string> RequestProxyTokenAsync()
        {
            var tokenResponse = await m_ProxyApi.GetTokenAsync();
            return tokenResponse.Data.Token;
        }

        async Task FulfillEnvironmentIdAsync()
        {
            if (!string.IsNullOrEmpty(ServerEnvironmentIdComponent.EnvironmentId))
                return;

            var environments = await m_EnvironmentApi.GetEnvironmentsAsync(m_CloudProjectId.GetCloudProjectId());

            var environmentResponse = environments.Data.Results.FirstOrDefault(x => x.Name == m_Environment.Current);

            if (environmentResponse == null)
            {
                throw new Exception($"No environment id found for environment '{m_Environment.Current}'");
            }

            ServerEnvironmentIdComponent.EnvironmentId = environmentResponse.Id.ToString();
        }

        public void ClearCredentials()
        {
            Reset();
        }

        internal void Reset()
        {
            ServerAccessTokenComponent.Clear();
            CancelScheduledRefresh();
            CancelScheduledExpiration();
            m_SessionGeneration++;
            ChangeState(ServerAuthenticationState.Unauthorized);
        }

        internal void ProcessResponse(string accessToken)
        {
            var decodedToken = m_JwtDecoder.Decode<ServerAccessToken>(accessToken);
            var expiresIn = decodedToken.ExpirationTimeUnix - decodedToken.IssuedAtTimeUnix;
            var refreshTime = expiresIn - Settings.AccessTokenRefreshBuffer;
            var expiryTime = expiresIn - Settings.AccessTokenExpiryBuffer;

            ServerAccessTokenComponent.AccessToken = accessToken;
            ServerAccessTokenComponent.ExpiryTime = m_DateTime.UtcNow.AddSeconds(expiryTime);

            var environmentId = decodedToken.Audience?.FirstOrDefault(s => s.StartsWith("envId:"))?.Substring(6);

            if (!string.IsNullOrEmpty(environmentId))
            {
                ServerEnvironmentIdComponent.EnvironmentId = environmentId;
                Logger.LogVerbose($"EnvironmentId: {environmentId}");
            }

            ScheduleRefresh(refreshTime);
            ScheduleExpiration(expiryTime);
            ChangeState(ServerAuthenticationState.Authorized);
        }

        internal void ScheduleRefresh(double delay)
        {
            CancelScheduledRefresh();

            if (m_DateTime.UtcNow.AddSeconds(delay) < ServerAccessTokenComponent.ExpiryTime)
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

            // The action scheduler runs this on the player loop, which is also what completes the web request:
            // waiting for the refresh to finish here would deadlock. Faults are logged instead of being swallowed.
            StartRefreshAsync().ContinueWith(task => Logger.LogException(task.Exception), TaskContinuationOptions.OnlyOnFaulted);
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
            ServerAccessTokenComponent.Clear();
            CancelScheduledRefresh();
            CancelScheduledExpiration();
            m_SessionGeneration++;
            ChangeState(ServerAuthenticationState.Expired);
        }

        internal async Task StartRefreshAsync()
        {
            if (!IsAuthorized)
                return;

            ChangeState(ServerAuthenticationState.Refreshing);

            var session = m_SessionGeneration;

            try
            {
                string accessToken;

                switch (AuthType)
                {
                    case AuthType.Proxy:
                        accessToken = await RequestProxyTokenAsync();
                        break;
                    case AuthType.ServiceAccount:
                        accessToken = await RequestServiceAccountTokenAsync(m_ServiceAuthApi.Configuration.Username, m_ServiceAuthApi.Configuration.Password, m_Scopes);
                        break;
                    default:
                        return;
                }

                if (session != m_SessionGeneration)
                {
                    // Cleared credentials or an expiration ended the session while the request was in flight.
                    // Applying the token now would undo that, or overwrite a newer sign-in.
                    Logger.LogVerbose($"Discarding a refreshed access token for an authorization session that has ended.");
                    return;
                }

                ProcessResponse(accessToken);
            }
            catch (Exception)
            {
                // The current token is still valid until its expiration is reached, so keep it and try again.
                ScheduleRefreshRetry();
            }
        }

        void ScheduleRefreshRetry()
        {
            if (State != ServerAuthenticationState.Refreshing)
                return;

            Logger.LogWarning("Failed to refresh access token due to network error or internal server error, will retry later.");
            ChangeState(ServerAuthenticationState.Authorized);
            ScheduleRefresh(Settings.RefreshAttemptFrequency);
        }

        internal void ChangeState(ServerAuthenticationState newState)
        {
            if (State == newState)
                return;

            Logger.LogVerbose($"Moved server from state [{State}] to [{newState}]");

            var oldState = State;
            State = newState;

            OnStateChanged(oldState, newState);
        }

        internal void SetServiceAccount(string apiKeyIdentifier, string apiKeySecret)
        {
            m_Configuration.SetServiceAccount(apiKeyIdentifier, apiKeySecret);
        }

        internal void OnStateChanged(ServerAuthenticationState oldState, ServerAuthenticationState newState)
        {
            StateChanged?.Invoke(oldState, newState);

            switch (newState)
            {
                case ServerAuthenticationState.Authorized:
                    if (oldState != ServerAuthenticationState.Refreshing)
                    {
                        Authorized?.Invoke();
                    }

                    break;

                case ServerAuthenticationState.Expired:
                    Expired?.Invoke();
                    break;
            }
        }

        void OnAuthorizationFailed(ServerAuthenticationException exception)
        {
            AuthorizationFailed?.Invoke(exception);
        }
    }
}
