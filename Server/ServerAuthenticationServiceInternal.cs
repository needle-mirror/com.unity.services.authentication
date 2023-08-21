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

        public async Task SignInWithServiceAccountAsync(string apiKeyIdentifier, string apiKeySecret)
        {
            if (State == ServerAuthenticationState.Unauthorized || State == ServerAuthenticationState.Expired)
            {
                try
                {
                    State = ServerAuthenticationState.SigningIn;
                    AuthType = AuthType.ServiceAccount;

                    if (string.IsNullOrEmpty(apiKeyIdentifier) || string.IsNullOrEmpty(apiKeySecret))
                        throw ServerAuthenticationException.Create(ServerAuthenticationErrorCodes.InvalidParameters, $"Invalid parameters.");

                    SetServiceAccount(apiKeyIdentifier, apiKeySecret);

                    if (string.IsNullOrEmpty(ServerEnvironmentIdComponent.EnvironmentId))
                    {
                        var environments = await m_EnvironmentApi.GetEnvironmentsAsync(m_CloudProjectId.GetCloudProjectId());

                        var environmentResponse = environments.Data.Results.FirstOrDefault(x => x.Name == m_Environment.Current);

                        if (environmentResponse == null)
                        {
                            throw new Exception($"No environment id found for environment '{m_Environment.Current}'");
                        }

                        ServerEnvironmentIdComponent.EnvironmentId = environmentResponse.Id.ToString();
                    }

                    var request = new ExchangeRequest(new List<string>());
                    var response = await m_ServiceAuthApi.ExchangeToStatelessAsync(m_CloudProjectId.GetCloudProjectId(), ServerEnvironmentIdComponent.EnvironmentId, request);

                    ProcessResponse(response.Data.AccessToken);
                }
                catch (ApiException apiException)
                {
                    var exception = ServerAuthenticationExceptionFactory.Create(apiException);
                    OnAuthorizationFailed(exception);
                    Reset();
                    throw exception;
                }
                catch (Exception unknownException)
                {
                    var exception = ServerAuthenticationExceptionFactory.Create(unknownException);
                    OnAuthorizationFailed(exception);
                    Reset();
                    throw exception;
                }
            }
            else
            {
                var exception = ServerAuthenticationExceptionFactory.CreateClientInvalidState(State);
                OnAuthorizationFailed(exception);
                throw exception;
            }
        }

        public async Task SignInFromServerAsync()
        {
            if (State == ServerAuthenticationState.Unauthorized || State == ServerAuthenticationState.Expired)
            {
                try
                {
                    State = ServerAuthenticationState.SigningIn;
                    AuthType = AuthType.Proxy;

                    var tokenResponse = await m_ProxyApi.GetTokenAsync();
                    ProcessResponse(tokenResponse.Data.Token);
                }
                catch (ApiException apiException)
                {
                    var exception = ServerAuthenticationExceptionFactory.Create(apiException);
                    OnAuthorizationFailed(exception);
                    Reset();
                    throw exception;
                }
                catch (Exception unknownException)
                {
                    var exception = ServerAuthenticationExceptionFactory.Create(unknownException);
                    OnAuthorizationFailed(exception);
                    Reset();
                    throw exception;
                }
            }
            else
            {
                var exception = ServerAuthenticationExceptionFactory.CreateClientInvalidState(State);
                OnAuthorizationFailed(exception);
                throw exception;
            }
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
            StartRefreshAsync().GetAwaiter().GetResult();
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
            ChangeState(ServerAuthenticationState.Expired);
        }

        internal async Task StartRefreshAsync()
        {
            if (IsAuthorized)
            {
                ChangeState(ServerAuthenticationState.Refreshing);

                try
                {
                    switch (AuthType)
                    {
                        case AuthType.Proxy:
                            await SignInFromServerAsync();
                            break;
                        case AuthType.ServiceAccount:
                            await SignInWithServiceAccountAsync(m_ServiceAuthApi.Configuration.Username, m_ServiceAuthApi.Configuration.Password);
                            break;
                    }
                }
                catch (Exception)
                {
                    if (State == ServerAuthenticationState.Refreshing)
                    {
                        Logger.LogWarning("Failed to refresh access token due to network error or internal server error, will retry later.");
                        ChangeState(ServerAuthenticationState.Authorized);
                        ScheduleRefresh(Settings.RefreshAttemptFrequency);
                    }
                }
            }
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
