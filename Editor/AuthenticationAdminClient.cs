using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Unity.Services.Authentication.Editor.Models;
using Unity.Services.Authentication.Models;
using Unity.Services.Authentication.Utilities;
using Unity.Services.Core;
using Unity.Services.Core.Internal;
using UnityEditor;
using UnityEngine;
using Logger = Unity.Services.Authentication.Utilities.Logger;

[assembly: InternalsVisibleTo("Unity.Services.Authentication.Editor.Tests")]
[assembly: InternalsVisibleTo("Unity.Services.Authentication.EditorTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")] // For Moq

namespace Unity.Services.Authentication.Editor
{
    static class AuthenticationAdminClientManager
    {
        internal static IAuthenticationAdminClient Create()
        {
            if (!IsConfigured())
                return null;

            var networkClient = new AuthenticationAdminNetworkClient("https://services.unity.com", GetOrganizationId(), GetProjectId(), new NetworkingUtilities(null));
            return new AuthenticationAdminClient(networkClient, new GenesisTokenProvider());
        }

        internal static bool IsConfigured()
        {
            return !string.IsNullOrEmpty(GetOrganizationId()) && !string.IsNullOrEmpty(GetProjectId());
        }

        // GetOrganizationId will gets the organization id associated with this Unity project.
        static string GetOrganizationId()
        {
            // This is a temporary workaround to get the Genesis organization foreign key for non-DevX enhanced Unity versions.
            // When the eventual changes are backported into previous versions of Unity, this will no longer be necessary.
            Assembly assembly = Assembly.GetAssembly(typeof(EditorWindow));
            var unityConnectInstance = assembly.CreateInstance("UnityEditor.Connect.UnityConnect", false, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null, null);
            Type t = unityConnectInstance.GetType();
            var projectInfo = t.GetProperty("projectInfo").GetValue(unityConnectInstance, null);

            Type projectInfoType = projectInfo.GetType();
            return projectInfoType.GetProperty("organizationForeignKey").GetValue(projectInfo, null) as string;
        }

        static string GetProjectId()
        {
            return CloudProjectSettings.projectId;
        }
    }

    class AuthenticationAdminClient : IAuthenticationAdminClient
    {
        readonly IAuthenticationAdminNetworkClient m_AuthenticationAdminNetworkClient;
        readonly IGenesisTokenProvider m_GenesisTokenProvider;
        string m_IdDomain;
        string m_ServicesGatewayToken;

        string GenesisToken => m_GenesisTokenProvider.Token;
        public string ServicesGatewayToken => m_ServicesGatewayToken;

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

        public IAsyncOperation<string> GetIDDomain()
        {
            var asyncOp = new AsyncOperation<string>();
            Action<string> getIdDomainFunc = token =>
            {
                var getDefaultIdDomainRequest = m_AuthenticationAdminNetworkClient.GetDefaultIdDomain(token);
                getDefaultIdDomainRequest.Completed += request => HandleGetIdDomainAPICall(asyncOp, request);
            };

            var tokenAsyncOp = ExchangeToken();
            tokenAsyncOp.Completed += tokenAsyncOpResult => getIdDomainFunc(tokenAsyncOpResult?.Result);
            return asyncOp;
        }

        public IAsyncOperation<IdProviderResponse> CreateIdProvider(string iddomain, CreateIdProviderRequest body)
        {
            var asyncOp = new AsyncOperation<IdProviderResponse>();
            Action<string> createIdProviderFunc = token =>
            {
                var request = m_AuthenticationAdminNetworkClient.CreateIdProvider(body, iddomain, token);
                request.Completed += req => HandleAuthenticationAdminApiCall(asyncOp, req);
            };

            var tokenAsyncOp = ExchangeToken();
            tokenAsyncOp.Completed += tokenAsyncOpResult => createIdProviderFunc(tokenAsyncOpResult?.Result);
            return asyncOp;
        }

        public IAsyncOperation<ListIdProviderResponse> ListIdProviders(string iddomain)
        {
            var asyncOp = new AsyncOperation<ListIdProviderResponse>();
            Action<string> listIdProviderFunc = token =>
            {
                var request = m_AuthenticationAdminNetworkClient.ListIdProvider(iddomain, token);
                request.Completed += req => HandleAuthenticationAdminApiCall(asyncOp, req);
            };

            var tokenAsyncOp = ExchangeToken();
            tokenAsyncOp.Completed += tokenAsyncOpResult => listIdProviderFunc(tokenAsyncOpResult?.Result);
            return asyncOp;
        }

        public IAsyncOperation<IdProviderResponse> UpdateIdProvider(string iddomain, string type, UpdateIdProviderRequest body)
        {
            var asyncOp = new AsyncOperation<IdProviderResponse>();
            Action<string> enableIdProviderFunc = token =>
            {
                var request = m_AuthenticationAdminNetworkClient.UpdateIdProvider(body, iddomain, type, token);
                request.Completed += req => HandleAuthenticationAdminApiCall(asyncOp, req);
            };

            var tokenAsyncOp = ExchangeToken();
            tokenAsyncOp.Completed += tokenAsyncOpResult => enableIdProviderFunc(tokenAsyncOpResult?.Result);
            return asyncOp;
        }

        public IAsyncOperation<IdProviderResponse> EnableIdProvider(string iddomain, string type)
        {
            var asyncOp = new AsyncOperation<IdProviderResponse>();
            Action<string> enableIdProviderFunc = token =>
            {
                var request = m_AuthenticationAdminNetworkClient.EnableIdProvider(iddomain, type, token);
                request.Completed += req => HandleAuthenticationAdminApiCall(asyncOp, req);
            };

            var tokenAsyncOp = ExchangeToken();
            tokenAsyncOp.Completed += tokenAsyncOpResult => enableIdProviderFunc(tokenAsyncOpResult?.Result);
            return asyncOp;
        }

        public IAsyncOperation<IdProviderResponse> DisableIdProvider(string iddomain, string type)
        {
            var asyncOp = new AsyncOperation<IdProviderResponse>();
            Action<string> disableIdProviderFunc = token =>
            {
                var request = m_AuthenticationAdminNetworkClient.DisableIdProvider(iddomain, type, token);
                request.Completed += req => HandleAuthenticationAdminApiCall(asyncOp, req);
            };

            var tokenAsyncOp = ExchangeToken();
            tokenAsyncOp.Completed += tokenAsyncOpResult => disableIdProviderFunc(tokenAsyncOpResult?.Result);
            return asyncOp;
        }

        public IAsyncOperation<IdProviderResponse> DeleteIdProvider(string iddomain, string type)
        {
            var asyncOp = new AsyncOperation<IdProviderResponse>();
            Action<string> deleteIdProviderFunc = token =>
            {
                var request = m_AuthenticationAdminNetworkClient.DeleteIdProvider(iddomain, type, token);
                request.Completed += req => HandleAuthenticationAdminApiCall(asyncOp, req);
            };

            var tokenAsyncOp = ExchangeToken();
            tokenAsyncOp.Completed += tokenAsyncOpResult => deleteIdProviderFunc(tokenAsyncOpResult?.Result);
            return asyncOp;
        }

        internal IAsyncOperation<string> ExchangeToken()
        {
            var asyncOp = new AsyncOperation<string>();
            var request = m_AuthenticationAdminNetworkClient.TokenExchange(GenesisToken);
            request.Completed += req => HandleTokenExchange(asyncOp, req);
            return asyncOp;
        }

        void HandleGetIdDomainAPICall(AsyncOperation<string> asyncOp, IWebRequest<GetIdDomainResponse> request)
        {
            if (HandleError(asyncOp, request, ServiceCalled.AuthenticationAdmin))
            {
                return;
            }

            m_IdDomain = request?.ResponseBody?.Id;
            asyncOp.Succeed(request?.ResponseBody?.Id);
        }

        void HandleTokenExchange(AsyncOperation<string> asyncOp, IWebRequest<TokenExchangeResponse> request)
        {
            if (HandleError(asyncOp, request, ServiceCalled.TokenExchange))
            {
                return;
            }

            var token = request?.ResponseBody?.Token;
            m_ServicesGatewayToken = token;
            asyncOp.Succeed(token);
        }

        void HandleAuthenticationAdminApiCall<T>(AsyncOperation<T> asyncOp, IWebRequest<T> request)
        {
            if (HandleError(asyncOp, request, ServiceCalled.AuthenticationAdmin))
            {
                return;
            }

            asyncOp.Succeed(request.ResponseBody);
        }

        internal bool HandleError<Q, T>(AsyncOperation<Q> asyncOp, IWebRequest<T> request, ServiceCalled sc)
        {
            if (!request.RequestFailed)
            {
                return false;
            }

            if (request.NetworkError)
            {
                asyncOp.Fail(AuthenticationException.Create(CommonErrorCodes.TransportError, "Network Error: " + request.ErrorMessage));
                return true;
            }
            Logger.LogError("Error message: " + request.ErrorMessage);

            try
            {
                switch (sc)
                {
                    case ServiceCalled.TokenExchange:
                        var tokenExchangeErrorResponse = JsonConvert.DeserializeObject<TokenExchangeErrorResponse>(request.ErrorMessage);
                        asyncOp.Fail(AuthenticationException.Create(MapErrorCodes(tokenExchangeErrorResponse.Name), tokenExchangeErrorResponse.Message));
                        break;
                    case ServiceCalled.AuthenticationAdmin:
                        var authenticationAdminErrorResponse = JsonConvert.DeserializeObject<AuthenticationErrorResponse>(request.ErrorMessage);
                        asyncOp.Fail(AuthenticationException.Create(MapErrorCodes(authenticationAdminErrorResponse.Title), authenticationAdminErrorResponse.Detail));
                        break;
                    default:
                        asyncOp.Fail(AuthenticationException.Create(CommonErrorCodes.Unknown, "Unknown error"));
                        break;
                }
            }
            catch (JsonException ex)
            {
                Logger.LogException(ex);
                asyncOp.Fail(AuthenticationException.Create(CommonErrorCodes.Unknown, "Failed to deserialize server response: " + request.ErrorMessage, ex));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                asyncOp.Fail(AuthenticationException.Create(CommonErrorCodes.Unknown, "Unknown error deserializing server response: " + request.ErrorMessage, ex));
            }

            return true;
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
            Debug.LogWarning("Unknown server error: " + serverErrorTitle);
            return CommonErrorCodes.Unknown;
        }
    }
}
