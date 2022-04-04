using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unity.Services.Authentication.Editor
{
    class AuthenticationAdminNetworkClient : IAuthenticationAdminNetworkClient
    {
        const string k_ServicesGatewayStem = "/api/player-identity/v1/organizations/";
        const string k_TokenExchangeStem = "/api/auth/v1/genesis-token-exchange/unity";

        readonly string m_ServicesGatewayHost;

        readonly string m_BaseIdDomainUrl;
        readonly string m_GetDefaultIdDomainUrl;
        readonly string m_TokenExchangeUrl;

        readonly string m_OrganizationId;

        readonly INetworkHandler m_NetworkHandler;

        readonly Dictionary<string, string> m_CommonPlayerIdentityHeaders;

        internal AuthenticationAdminNetworkClient(string servicesGatewayHost,
                                                  string organizationId,
                                                  string projectId,
                                                  INetworkHandler networkHandler)
        {
            m_ServicesGatewayHost = servicesGatewayHost;
            m_OrganizationId = organizationId;
            m_BaseIdDomainUrl = $"{m_ServicesGatewayHost}{k_ServicesGatewayStem}{m_OrganizationId}/iddomains";
            m_GetDefaultIdDomainUrl = $"{m_BaseIdDomainUrl}/default";
            m_TokenExchangeUrl = $"{m_ServicesGatewayHost}{k_TokenExchangeStem}";
            m_NetworkHandler = networkHandler;

            m_CommonPlayerIdentityHeaders = new Dictionary<string, string>
            {
                ["ProjectId"] = projectId,
                // The Error-Version header enables RFC7807HttpError error responses
                ["Error-Version"] = "v1"
            };
        }

        public Task<GetIdDomainResponse> GetDefaultIdDomainAsync(string token)
        {
            return m_NetworkHandler.GetAsync<GetIdDomainResponse>(m_GetDefaultIdDomainUrl, AddTokenHeader(CreateCommonHeaders(), token));
        }

        public Task<TokenExchangeResponse> ExchangeTokenAsync(string token)
        {
            var body = new TokenExchangeRequest();
            body.Token = token;
            return m_NetworkHandler.PostAsync<TokenExchangeResponse>(m_TokenExchangeUrl, body);
        }

        public Task<IdProviderResponse> CreateIdProviderAsync(CreateIdProviderRequest body, string idDomain, string token)
        {
            return m_NetworkHandler.PostAsync<IdProviderResponse>(GetIdProviderUrl(idDomain), body, AddTokenHeader(CreateCommonHeaders(), token));
        }

        public Task<ListIdProviderResponse> ListIdProviderAsync(string idDomain, string token)
        {
            return m_NetworkHandler.GetAsync<ListIdProviderResponse>(GetIdProviderUrl(idDomain), AddTokenHeader(CreateCommonHeaders(), token));
        }

        public Task<IdProviderResponse> UpdateIdProviderAsync(UpdateIdProviderRequest body, string idDomain, string type, string token)
        {
            return m_NetworkHandler.PutAsync<IdProviderResponse>(GetIdProviderTypeUrl(idDomain, type), body, AddTokenHeader(CreateCommonHeaders(), token));
        }

        public Task<IdProviderResponse> EnableIdProviderAsync(string idDomain, string type, string token)
        {
            return m_NetworkHandler.PostAsync<IdProviderResponse>(GetEnableIdProviderTypeUrl(idDomain, type), AddJsonHeader(AddTokenHeader(CreateCommonHeaders(), token)));
        }

        public Task<IdProviderResponse> DisableIdProviderAsync(string idDomain, string type, string token)
        {
            return m_NetworkHandler.PostAsync<IdProviderResponse>(GetDisableIdProviderTypeUrl(idDomain, type), AddJsonHeader(AddTokenHeader(CreateCommonHeaders(), token)));
        }

        public Task<IdProviderResponse> DeleteIdProviderAsync(string idDomain, string type, string token)
        {
            return m_NetworkHandler.DeleteAsync<IdProviderResponse>(GetIdProviderTypeUrl(idDomain, type), AddTokenHeader(CreateCommonHeaders(), token));
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

        string GetEnableIdProviderTypeUrl(string idDomain, string type)
        {
            return $"{GetIdProviderTypeUrl(idDomain, type)}/enable";
        }

        string GetDisableIdProviderTypeUrl(string idDomain, string type)
        {
            return $"{GetIdProviderTypeUrl(idDomain, type)}/disable";
        }

        string GetIdProviderTypeUrl(string idDomain, string type)
        {
            return $"{GetIdProviderUrl(idDomain)}/{type}";
        }

        string GetIdProviderUrl(string idDomain)
        {
            return $"{m_BaseIdDomainUrl}/{idDomain}/idps";
        }
    }
}
