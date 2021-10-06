using System.Collections.Generic;
using Unity.Services.Authentication.Editor.Models;
using Unity.Services.Authentication.Utilities;

namespace Unity.Services.Authentication.Editor
{
    interface IAuthenticationAdminNetworkClient
    {
        IWebRequest<TokenExchangeResponse> TokenExchange(string token);
        IWebRequest<GetIdDomainResponse> GetDefaultIdDomain(string token);
        IWebRequest<IdProviderResponse> CreateIdProvider(CreateIdProviderRequest body, string idDomain, string token);
        IWebRequest<ListIdProviderResponse> ListIdProvider(string idDomain, string token);
        IWebRequest<IdProviderResponse> UpdateIdProvider(UpdateIdProviderRequest body, string idDomain, string type, string token);
        IWebRequest<IdProviderResponse> EnableIdProvider(string idDomain, string type, string token);
        IWebRequest<IdProviderResponse> DisableIdProvider(string idDomain, string type, string token);
        IWebRequest<IdProviderResponse> DeleteIdProvider(string idDomain, string type, string token);
    }

    class AuthenticationAdminNetworkClient : IAuthenticationAdminNetworkClient
    {
        const string k_ServicesGatewayStem = "/api/player-identity/v1/organizations/";
        const string k_TokenExchangeStem = "/api/auth/v1/genesis-token-exchange/unity";

        readonly string m_ServicesGatewayHost;

        readonly string m_BaseIdDomainUrl;
        readonly string m_GetDefaultIdDomainUrl;
        readonly string m_TokenExchangeUrl;

        readonly string m_OrganizationId;
        readonly string m_ProjectId;

        readonly INetworkingUtilities m_NetworkClient;

        readonly Dictionary<string, string> m_CommonPlayerIdentityHeaders;

        internal AuthenticationAdminNetworkClient(string servicesGatewayHost,
                                                  string organizationId,
                                                  string projectId,
                                                  INetworkingUtilities networkClient)
        {
            m_ServicesGatewayHost = servicesGatewayHost;
            m_OrganizationId = organizationId;
            m_ProjectId = projectId;

            m_BaseIdDomainUrl = $"{m_ServicesGatewayHost}{k_ServicesGatewayStem}{m_OrganizationId}/iddomains";
            m_GetDefaultIdDomainUrl = $"{m_BaseIdDomainUrl}/default";
            m_TokenExchangeUrl = $"{m_ServicesGatewayHost}{k_TokenExchangeStem}";
            m_NetworkClient = networkClient;

            m_CommonPlayerIdentityHeaders = new Dictionary<string, string>
            {
                ["ProjectId"] = projectId,
                // The Error-Version header enables RFC7807HttpError error responses
                ["Error-Version"] = "v1"
            };
        }

        public IWebRequest<GetIdDomainResponse> GetDefaultIdDomain(string token)
        {
            return m_NetworkClient.Get<GetIdDomainResponse>(m_GetDefaultIdDomainUrl, AddTokenHeader(CreateCommonHeaders(), token));
        }

        public IWebRequest<TokenExchangeResponse> TokenExchange(string token)
        {
            var body = new TokenExchangeRequest();
            body.Token = token;
            return m_NetworkClient.PostJson<TokenExchangeResponse>(m_TokenExchangeUrl, body);
        }

        public IWebRequest<IdProviderResponse> CreateIdProvider(CreateIdProviderRequest body, string idDomain, string token)
        {
            return m_NetworkClient.PostJson<IdProviderResponse>(GetIdProviderUrl(idDomain), body, AddTokenHeader(CreateCommonHeaders(), token));
        }

        public IWebRequest<ListIdProviderResponse> ListIdProvider(string idDomain, string token)
        {
            return m_NetworkClient.Get<ListIdProviderResponse>(GetIdProviderUrl(idDomain), AddTokenHeader(CreateCommonHeaders(), token));
        }

        public IWebRequest<IdProviderResponse> UpdateIdProvider(UpdateIdProviderRequest body, string idDomain, string type, string token)
        {
            return m_NetworkClient.Put<IdProviderResponse>(GetIdProviderTypeUrl(idDomain, type), body, AddTokenHeader(CreateCommonHeaders(), token));
        }

        public IWebRequest<IdProviderResponse> EnableIdProvider(string idDomain, string type, string token)
        {
            return m_NetworkClient.Post<IdProviderResponse>(GetEnableIdProviderTypeUrl(idDomain, type), AddJsonHeader(AddTokenHeader(CreateCommonHeaders(), token)));
        }

        public IWebRequest<IdProviderResponse> DisableIdProvider(string idDomain, string type, string token)
        {
            return m_NetworkClient.Post<IdProviderResponse>(GetDisableIdProviderTypeUrl(idDomain, type), AddJsonHeader(AddTokenHeader(CreateCommonHeaders(), token)));
        }

        public IWebRequest<IdProviderResponse> DeleteIdProvider(string idDomain, string type, string token)
        {
            return m_NetworkClient.Delete<IdProviderResponse>(GetIdProviderTypeUrl(idDomain, type), AddTokenHeader(CreateCommonHeaders(), token));
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
