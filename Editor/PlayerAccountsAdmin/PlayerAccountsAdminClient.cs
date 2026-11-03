using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unity.Services.Authentication.Editor
{
    class PlayerAccountsAdminClient : IPlayerAccountsAdminClient
    {
        // The Player Accounts admin routes, from unity-gateway's openapi-specs/services/player-accounts/admin-v1-routes.yaml.
        const string k_ServicesGatewayStem = "/api/player-accounts/v1";
        const string k_ClientsPath = "clients";
        const string k_IdentityProvidersPath = "idps";

        readonly string m_ProjectUrl;
        readonly INetworkHandler m_NetworkHandler;
        // The Authentication (player identity) admin API: the Services Gateway token exchange, and the project's
        // sign-in providers, which hold the Unity Player Accounts entry.
        readonly IAuthenticationAdminNetworkClient m_AuthenticationAdmin;
        readonly IGenesisTokenProvider m_GenesisTokenProvider;

        string m_GatewayToken;

        public string ProjectId { get; }

        internal PlayerAccountsAdminClient(string servicesGatewayHost,
                                           string projectId,
                                           INetworkHandler networkHandler,
                                           IAuthenticationAdminNetworkClient authenticationAdmin,
                                           IGenesisTokenProvider genesisTokenProvider)
        {
            ProjectId = projectId;
            m_ProjectUrl = $"{servicesGatewayHost}{k_ServicesGatewayStem}/projects/{Uri.EscapeDataString(projectId)}";
            m_NetworkHandler = networkHandler;
            m_AuthenticationAdmin = authenticationAdmin;
            m_GenesisTokenProvider = genesisTokenProvider;
        }

        public Task CreateOrUpdateClientAsync(PlayerAccountsClientRequest request)
        {
            // 202 Accepted with no body.
            return SendAsync(token => m_NetworkHandler.PostAsync<object>(ClientsUrl, request, Headers(token)));
        }

        public Task<PlayerAccountsClientResponse> GetClientAsync(string clientId)
        {
            return SendAsync(token => m_NetworkHandler.GetAsync<PlayerAccountsClientResponse>(ClientUrl(clientId), Headers(token)));
        }

        public Task<PlayerAccountsClientResponse> UpdateClientAsync(string clientId, PlayerAccountsClientRequest request)
        {
            return SendAsync(token => m_NetworkHandler.PutAsync<PlayerAccountsClientResponse>(ClientUrl(clientId), request, Headers(token)));
        }

        public async Task<IdProviderResponse> GetUnitySignInProviderAsync()
        {
            var providers = await SendAsync(token => m_AuthenticationAdmin.ListIdProviderAsync(ProjectId, token));
            if (providers?.Results == null)
            {
                return null;
            }

            foreach (var provider in providers.Results)
            {
                if (provider != null && string.Equals(provider.Type, IdProviderKeys.Unity, StringComparison.OrdinalIgnoreCase))
                {
                    return provider;
                }
            }

            return null;
        }

        public Task<IdProviderResponse> EnableUnitySignInProviderAsync()
        {
            return SendAsync(token => m_AuthenticationAdmin.EnableIdProviderAsync(ProjectId, IdProviderKeys.Unity, token));
        }

        public async Task<PlayerAccountsIdentityProviderResponse[]> ListIdentityProvidersAsync()
        {
            var response = await SendAsync(token => m_NetworkHandler.GetAsync<ListPlayerAccountsIdentityProvidersResponse>(IdentityProvidersUrl, Headers(token)));
            return response?.Results ?? Array.Empty<PlayerAccountsIdentityProviderResponse>();
        }

        public Task<PlayerAccountsIdentityProviderResponse> CreateIdentityProviderAsync(CreatePlayerAccountsIdentityProviderRequest request)
        {
            return SendAsync(token => m_NetworkHandler.PostAsync<PlayerAccountsIdentityProviderResponse>(IdentityProvidersUrl, request, Headers(token)));
        }

        public Task<PlayerAccountsIdentityProviderResponse> UpdateIdentityProviderAsync(string id, UpdatePlayerAccountsIdentityProviderRequest request)
        {
            return SendAsync(token => m_NetworkHandler.PutAsync<PlayerAccountsIdentityProviderResponse>(IdentityProviderUrl(id), request, Headers(token)));
        }

        public Task DeleteIdentityProviderAsync(string id)
        {
            // 204 No Content.
            return SendAsync(token => m_NetworkHandler.DeleteAsync<object>(IdentityProviderUrl(id), Headers(token)));
        }

        internal string ClientsUrl => $"{m_ProjectUrl}/{k_ClientsPath}";

        internal string ClientUrl(string clientId) => $"{ClientsUrl}/{Uri.EscapeDataString(clientId)}";

        internal string IdentityProvidersUrl => $"{m_ProjectUrl}/{k_IdentityProvidersPath}";

        internal string IdentityProviderUrl(string id) => $"{IdentityProvidersUrl}/{Uri.EscapeDataString(id)}";

        Dictionary<string, string> Headers(string token)
        {
            return new Dictionary<string, string>
            {
                ["ProjectId"] = ProjectId,
                // The Error-Version header enables RFC7807HttpError error responses
                ["Error-Version"] = "v1",
                ["Authorization"] = "Bearer " + token,
            };
        }

        async Task<T> SendAsync<T>(Func<string, Task<T>> send)
        {
            var token = await GetGatewayTokenAsync();
            try
            {
                return await send(token);
            }
            catch (WebRequestException e)
            {
                if (e.ResponseCode == 401)
                {
                    m_GatewayToken = null;
                }

                throw Classify(e);
            }
        }

        async Task<string> GetGatewayTokenAsync()
        {
            if (!string.IsNullOrEmpty(m_GatewayToken))
            {
                return m_GatewayToken;
            }

            var genesisToken = m_GenesisTokenProvider.Token;
            if (string.IsNullOrEmpty(genesisToken))
            {
                throw new PlayerAccountsAdminException(PlayerAccountsAdminException.FailureReason.Unauthorized, 0, null);
            }

            try
            {
                var response = await m_AuthenticationAdmin.ExchangeTokenAsync(genesisToken);
                m_GatewayToken = response?.Token;
                return m_GatewayToken;
            }
            catch (WebRequestException e)
            {
                // The exchange refusing the Editor's own token means its sign-in is no longer valid. A network or
                // server failure says nothing about the sign-in, so it keeps its own classification.
                if (e.NetworkError || e.ResponseCode >= 500)
                {
                    throw Classify(e);
                }

                throw new PlayerAccountsAdminException(PlayerAccountsAdminException.FailureReason.Unauthorized, e.ResponseCode, ServerDetail(e.Message), e);
            }
        }

        internal static PlayerAccountsAdminException Classify(WebRequestException exception)
        {
            if (exception.NetworkError)
            {
                return new PlayerAccountsAdminException(PlayerAccountsAdminException.FailureReason.Network, 0, exception.Message, exception);
            }

            var detail = ServerDetail(exception.Message);
            return new PlayerAccountsAdminException(ReasonFor(exception.ResponseCode), exception.ResponseCode, detail, exception);
        }

        internal static PlayerAccountsAdminException.FailureReason ReasonFor(long statusCode)
        {
            switch (statusCode)
            {
                case 401:
                    return PlayerAccountsAdminException.FailureReason.Unauthorized;
                case 403:
                    return PlayerAccountsAdminException.FailureReason.Forbidden;
                case 404:
                    return PlayerAccountsAdminException.FailureReason.NotFound;
                case 400:
                case 409:
                case 422:
                    return PlayerAccountsAdminException.FailureReason.InvalidRequest;
                default:
                    return PlayerAccountsAdminException.FailureReason.Unknown;
            }
        }

        // Error responses are RFC 7807 problem details. Prefer their human-readable detail over the raw body.
        internal static string ServerDetail(string body)
        {
            if (string.IsNullOrEmpty(body))
            {
                return null;
            }

            try
            {
                var problem = IsolatedJsonConvert.DeserializeObject<AuthenticationErrorResponse>(body, SerializerSettings.DefaultSerializerSettings);
                if (!string.IsNullOrEmpty(problem?.Detail))
                {
                    return problem.Detail;
                }

                if (!string.IsNullOrEmpty(problem?.Title))
                {
                    return problem.Title;
                }
            }
            catch (Exception)
            {
                // Not a problem-details body: fall back to the raw text.
            }

            return body;
        }
    }
}
