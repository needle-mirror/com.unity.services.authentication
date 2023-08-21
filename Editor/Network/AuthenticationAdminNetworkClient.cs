using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unity.Services.Authentication.Editor
{
    class AuthenticationAdminNetworkClient : IAuthenticationAdminNetworkClient
    {
        const string k_ServicesGatewayStem = "/api/player-identity/v1";
        const string k_TokenExchangeStem = "/api/auth/v1/genesis-token-exchange/unity";

        readonly string m_ServicesGatewayHost;

        readonly string m_BaseProjectIdUrl;
        readonly string m_TokenExchangeUrl;

        readonly INetworkHandler m_NetworkHandler;

        readonly Dictionary<string, string> m_CommonPlayerIdentityHeaders;

        internal AuthenticationAdminNetworkClient(string servicesGatewayHost,
                                                  string projectId,
                                                  INetworkHandler networkHandler)
        {
            m_ServicesGatewayHost = servicesGatewayHost;
            m_BaseProjectIdUrl = $"{m_ServicesGatewayHost}{k_ServicesGatewayStem}/projects";
            m_TokenExchangeUrl = $"{m_ServicesGatewayHost}{k_TokenExchangeStem}";
            m_NetworkHandler = networkHandler;

            m_CommonPlayerIdentityHeaders = new Dictionary<string, string>
            {
                ["ProjectId"] = projectId,
                // The Error-Version header enables RFC7807HttpError error responses
                ["Error-Version"] = "v1"
            };
        }

        public Task<TokenExchangeResponse> ExchangeTokenAsync(string token)
        {
            var body = new TokenExchangeRequest();
            body.Token = token;
            return m_NetworkHandler.PostAsync<TokenExchangeResponse>(m_TokenExchangeUrl, body);
        }

        public Task<IdProviderResponse> CreateIdProviderAsync(CreateIdProviderRequest body, string projectId, string token)
        {
            return m_NetworkHandler.PostAsync<IdProviderResponse>(GetIdProviderUrl(projectId), body, AddTokenHeader(CreateCommonHeaders(), token));
        }

        public Task<ListIdProviderResponse> ListIdProviderAsync(string projectId, string token)
        {
            return m_NetworkHandler.GetAsync<ListIdProviderResponse>(GetIdProviderUrl(projectId), AddTokenHeader(CreateCommonHeaders(), token));
        }

        public Task<IdProviderResponse> UpdateIdProviderAsync(UpdateIdProviderRequest body, string projectId, string type, string token)
        {
            return m_NetworkHandler.PutAsync<IdProviderResponse>(GetIdProviderTypeUrl(projectId, type), body, AddTokenHeader(CreateCommonHeaders(), token));
        }

        public Task<IdProviderResponse> EnableIdProviderAsync(string projectId, string type, string token)
        {
            return m_NetworkHandler.PostAsync<IdProviderResponse>(GetEnableIdProviderTypeUrl(projectId, type), AddJsonHeader(AddTokenHeader(CreateCommonHeaders(), token)));
        }

        public Task<IdProviderResponse> DisableIdProviderAsync(string projectId, string type, string token)
        {
            return m_NetworkHandler.PostAsync<IdProviderResponse>(GetDisableIdProviderTypeUrl(projectId, type), AddJsonHeader(AddTokenHeader(CreateCommonHeaders(), token)));
        }

        public Task<IdProviderResponse> DeleteIdProviderAsync(string projectId, string type, string token)
        {
            return m_NetworkHandler.DeleteAsync<IdProviderResponse>(GetIdProviderTypeUrl(projectId, type), AddTokenHeader(CreateCommonHeaders(), token));
        }

        Dictionary<string, string> CreateCommonHeaders()
        {
            return new Dictionary<string, string>(m_CommonPlayerIdentityHeaders);
        }

        Dictionary<string, string> AddTokenHeader(Dictionary<string, string> headers, string token)
        {
            headers.Add("Authorization", "Bearer " + token);
            return headers;
        }

        Dictionary<string, string> AddJsonHeader(Dictionary<string, string> headers)
        {
            headers.Add("Content-Type", "application/json");
            return headers;
        }

        string GetEnableIdProviderTypeUrl(string projectId, string type)
        {
            return $"{GetIdProviderTypeUrl(projectId, type)}/enable";
        }

        string GetDisableIdProviderTypeUrl(string projectId, string type)
        {
            return $"{GetIdProviderTypeUrl(projectId, type)}/disable";
        }

        string GetIdProviderTypeUrl(string projectId, string type)
        {
            return $"{GetIdProviderUrl(projectId)}/{type}";
        }

        string GetIdProviderUrl(string projectId)
        {
            return $"{m_BaseProjectIdUrl}/{projectId}/idps";
        }
    }
}
