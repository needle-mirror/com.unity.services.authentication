using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Environments.Internal;

namespace Unity.Services.Authentication
{
    class AuthenticationNetworkClient : IAuthenticationNetworkClient
    {
        const string k_WellKnownUrlStem = "/.well-known/jwks.json";
        const string k_AnonymousUrlStem = "/v1/authentication/anonymous";
        const string k_SessionTokenUrlStem = "/v1/authentication/session-token";
        const string k_ExternalTokenUrlStem = "/v1/authentication/external-token";
        const string k_LinkExternalTokenUrlStem = "/v1/authentication/link";
        const string k_UnlinkExternalTokenUrlStem = "/v1/authentication/unlink";
        const string k_UsersUrlStem = "/v1/users";

        internal AccessTokenComponent AccessTokenComponent { get; }
        internal ICloudProjectId CloudProjectIdComponent { get; }
        internal IEnvironments EnvironmentComponent { get; }
        internal INetworkHandler NetworkHandler { get; }

        string AccessToken => AccessTokenComponent.AccessToken;
        string EnvironmentId => EnvironmentComponent.Current;

        readonly string m_WellKnownUrl;
        readonly string m_AnonymousUrl;
        readonly string m_SessionTokenUrl;
        readonly string m_ExternalTokenUrl;
        readonly string m_LinkExternalTokenUrl;
        readonly string m_UnlinkExternalTokenUrl;
        readonly string m_UsersUrl;

        readonly Dictionary<string, string> m_CommonHeaders;

        internal AuthenticationNetworkClient(string host,
                                             ICloudProjectId cloudProjectId,
                                             IEnvironments environment,
                                             INetworkHandler networkHandler,
                                             AccessTokenComponent accessToken)
        {
            AccessTokenComponent = accessToken;
            CloudProjectIdComponent = cloudProjectId;
            EnvironmentComponent = environment;
            NetworkHandler = networkHandler;

            m_WellKnownUrl = host + k_WellKnownUrlStem;
            m_AnonymousUrl = host + k_AnonymousUrlStem;
            m_SessionTokenUrl = host + k_SessionTokenUrlStem;
            m_ExternalTokenUrl = host + k_ExternalTokenUrlStem;
            m_LinkExternalTokenUrl = host + k_LinkExternalTokenUrlStem;
            m_UnlinkExternalTokenUrl = host + k_UnlinkExternalTokenUrlStem;
            m_UsersUrl = host + k_UsersUrlStem;


            m_CommonHeaders = new Dictionary<string, string>
            {
                ["ProjectId"] = CloudProjectIdComponent.GetCloudProjectId(),
                // The Error-Version header enables RFC7807HttpError error responses
                ["Error-Version"] = "v1"
            };
        }

        public Task<WellKnownKeysResponse> GetWellKnownKeysAsync()
        {
            return NetworkHandler.GetAsync<WellKnownKeysResponse>(m_WellKnownUrl);
        }

        public Task<SignInResponse> SignInAnonymouslyAsync()
        {
            return NetworkHandler.PostAsync<SignInResponse>(m_AnonymousUrl, WithEnvironment(GetCommonHeaders()));
        }

        public Task<SignInResponse> SignInWithSessionTokenAsync(string token)
        {
            return NetworkHandler.PostAsync<SignInResponse>(m_SessionTokenUrl, new SessionTokenRequest
            {
                SessionToken = token
            }, WithEnvironment(GetCommonHeaders()));
        }

        public Task<SignInResponse> SignInWithExternalTokenAsync(string idProvider, SignInWithExternalTokenRequest externalToken)
        {
            var url = $"{m_ExternalTokenUrl}/{idProvider}";
            return NetworkHandler.PostAsync<SignInResponse>(url, externalToken, WithEnvironment(GetCommonHeaders()));
        }

        public Task<LinkResponse> LinkWithExternalTokenAsync(string idProvider, LinkWithExternalTokenRequest externalToken)
        {
            var url = $"{m_LinkExternalTokenUrl}/{idProvider}";
            return NetworkHandler.PostAsync<LinkResponse>(url, externalToken, WithEnvironment(WithAccessToken(GetCommonHeaders())));
        }

        public Task<UnlinkResponse> UnlinkExternalTokenAsync(string idProvider, UnlinkRequest request)
        {
            var url = $"{m_UnlinkExternalTokenUrl}/{idProvider}";
            return NetworkHandler.PostAsync<UnlinkResponse>(url, request, WithEnvironment(WithAccessToken(GetCommonHeaders())));
        }

        public Task<PlayerInfoResponse> GetPlayerInfoAsync(string playerId)
        {
            return NetworkHandler.GetAsync<PlayerInfoResponse>(CreateUserRequestUrl(playerId), WithAccessToken(GetCommonHeaders()));
        }

        public Task DeleteAccountAsync(string playerId)
        {
            return NetworkHandler.DeleteAsync(CreateUserRequestUrl(playerId), WithEnvironment(WithAccessToken(GetCommonHeaders())));
        }

        string CreateUserRequestUrl(string user)
        {
            return $"{m_UsersUrl}/{user}";
        }

        Dictionary<string, string> WithAccessToken(Dictionary<string, string> headers)
        {
            headers["Authorization"] = $"Bearer {AccessToken}";
            return headers;
        }

        Dictionary<string, string> WithEnvironment(Dictionary<string, string> headers)
        {
            var environmentId = EnvironmentId;

            if (!string.IsNullOrEmpty(environmentId))
            {
                headers["UnityEnvironment"] = environmentId;
            }

            return headers;
        }

        Dictionary<string, string> GetCommonHeaders()
        {
            return new Dictionary<string, string>(m_CommonHeaders);
        }
    }
}
