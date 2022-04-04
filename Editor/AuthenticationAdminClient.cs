using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Core;
using Unity.Services.Core.Editor.OrganizationHandler;
using UnityEditor;

namespace Unity.Services.Authentication.Editor
{
    class AuthenticationAdminClient : IAuthenticationAdminClient
    {
        readonly IAuthenticationAdminNetworkClient m_AuthenticationAdminNetworkClient;
        readonly IGenesisTokenProvider m_GenesisTokenProvider;
        string m_IdDomain;

        string GenesisToken => m_GenesisTokenProvider.Token;
        public string GatewayToken { get; internal set; }

        internal enum ServiceCalled
        {
            TokenExchange,
            AuthenticationAdmin
        }

        public AuthenticationAdminClient(IAuthenticationAdminNetworkClient networkClient, IGenesisTokenProvider genesisTokenProvider)
        {
            m_AuthenticationAdminNetworkClient = networkClient;
            m_GenesisTokenProvider = genesisTokenProvider;
        }

        public async Task<string> GetIDDomainAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(GatewayToken))
                {
                    await ExchangeTokenAsync();
                }

                var response = await m_AuthenticationAdminNetworkClient.GetDefaultIdDomainAsync(GatewayToken);
                m_IdDomain = response?.Id;
                return m_IdDomain;
            }
            catch (WebRequestException e)
            {
                throw BuildException(e, ServiceCalled.AuthenticationAdmin);
            }
        }

        public async Task<IdProviderResponse> CreateIdProviderAsync(string idDomain, CreateIdProviderRequest body)
        {
            try
            {
                if (string.IsNullOrEmpty(GatewayToken))
                {
                    await ExchangeTokenAsync();
                }

                var response = await m_AuthenticationAdminNetworkClient.CreateIdProviderAsync(body, idDomain, GatewayToken);
                return response;
            }
            catch (WebRequestException e)
            {
                throw BuildException(e, ServiceCalled.AuthenticationAdmin);
            }
        }

        public async Task<ListIdProviderResponse> ListIdProvidersAsync(string idDomain)
        {
            try
            {
                if (string.IsNullOrEmpty(GatewayToken))
                {
                    await ExchangeTokenAsync();
                }

                var response = await m_AuthenticationAdminNetworkClient.ListIdProviderAsync(idDomain, GatewayToken);
                return response;
            }
            catch (WebRequestException e)
            {
                throw BuildException(e, ServiceCalled.AuthenticationAdmin);
            }
        }

        public async Task<IdProviderResponse> UpdateIdProviderAsync(string idDomain, string type, UpdateIdProviderRequest body)
        {
            try
            {
                if (string.IsNullOrEmpty(GatewayToken))
                {
                    await ExchangeTokenAsync();
                }

                var response = await m_AuthenticationAdminNetworkClient.UpdateIdProviderAsync(body, idDomain, type, GatewayToken);
                return response;
            }
            catch (WebRequestException e)
            {
                throw BuildException(e, ServiceCalled.AuthenticationAdmin);
            }
        }

        public async Task<IdProviderResponse> EnableIdProviderAsync(string idDomain, string type)
        {
            try
            {
                if (string.IsNullOrEmpty(GatewayToken))
                {
                    await ExchangeTokenAsync();
                }

                var response = await m_AuthenticationAdminNetworkClient.EnableIdProviderAsync(idDomain, type, GatewayToken);
                return response;
            }
            catch (WebRequestException e)
            {
                throw BuildException(e, ServiceCalled.AuthenticationAdmin);
            }
        }

        public async Task<IdProviderResponse> DisableIdProviderAsync(string idDomain, string type)
        {
            try
            {
                if (string.IsNullOrEmpty(GatewayToken))
                {
                    await ExchangeTokenAsync();
                }

                var response = await m_AuthenticationAdminNetworkClient.DisableIdProviderAsync(idDomain, type, GatewayToken);
                return response;
            }
            catch (WebRequestException e)
            {
                throw BuildException(e, ServiceCalled.AuthenticationAdmin);
            }
        }

        public async Task<IdProviderResponse> DeleteIdProviderAsync(string idDomain, string type)
        {
            try
            {
                if (string.IsNullOrEmpty(GatewayToken))
                {
                    await ExchangeTokenAsync();
                }

                var response = await m_AuthenticationAdminNetworkClient.DeleteIdProviderAsync(idDomain, type, GatewayToken);
                return response;
            }
            catch (WebRequestException e)
            {
                throw BuildException(e, ServiceCalled.AuthenticationAdmin);
            }
        }

        internal async Task ExchangeTokenAsync()
        {
            try
            {
                var response = await m_AuthenticationAdminNetworkClient.ExchangeTokenAsync(GenesisToken);
                GatewayToken = response.Token;
            }
            catch (WebRequestException e)
            {
                throw BuildException(e, ServiceCalled.TokenExchange);
            }
        }

        internal RequestFailedException BuildException(WebRequestException exception, ServiceCalled service)
        {
            if (exception.NetworkError)
            {
                return AuthenticationException.Create(CommonErrorCodes.TransportError, "Network Error: " + exception.Message);
            }

            if (exception.DeserializationError)
            {
                return AuthenticationException.Create(CommonErrorCodes.Unknown, "Deserialization Error: " + exception.Message);
            }

            Logger.LogError($"Error message: {exception.Message}");

            try
            {
                switch (service)
                {
                    case ServiceCalled.TokenExchange:
                        var tokenExchangeErrorResponse = JsonConvert.DeserializeObject<TokenExchangeErrorResponse>(exception.Message);
                        return AuthenticationException.Create(MapErrorCodes(tokenExchangeErrorResponse.Name), tokenExchangeErrorResponse.Message);
                    case ServiceCalled.AuthenticationAdmin:
                        var authenticationAdminErrorResponse = JsonConvert.DeserializeObject<AuthenticationErrorResponse>(exception.Message);
                        if (authenticationAdminErrorResponse.Status == 401)
                        {
                            GatewayToken = null;
                        }
                        return AuthenticationException.Create(MapErrorCodes(authenticationAdminErrorResponse.Title), authenticationAdminErrorResponse.Detail);
                    default:
                        return AuthenticationException.Create(CommonErrorCodes.Unknown, "Unknown error");
                }
            }
            catch (JsonException ex)
            {
                Logger.LogException(ex);
                return AuthenticationException.Create(CommonErrorCodes.Unknown, "Failed to deserialize server response: " + exception.Message, ex);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return AuthenticationException.Create(CommonErrorCodes.Unknown, "Unknown error deserializing server response: " + exception.Message, ex);
            }
        }

        static int MapErrorCodes(string serverErrorTitle)
        {
            switch (serverErrorTitle)
            {
                case "INVALID_PARAMETERS":
                    return AuthenticationErrorCodes.InvalidParameters;
                case "UNAUTHORIZED_REQUEST":
                    // This happens when either the token is invalid or the token has expired.
                    return CommonErrorCodes.InvalidToken;
            }

            Logger.LogWarning("Unknown server error: " + serverErrorTitle);
            return CommonErrorCodes.Unknown;
        }
    }
}
