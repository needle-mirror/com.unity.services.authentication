using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Authentication.Models;
using Unity.Services.Authentication.Utilities;
using Unity.Services.Core.Environments.Internal;

namespace Unity.Services.Authentication
{
    interface IAuthenticationNetworkClient
    {
        Task<WellKnownKeys> GetWellKnownKeysAsync();
        Task<SignInResponse> SignInAnonymouslyAsync();
        Task<SignInResponse> SignInWithSessionTokenAsync(string token);
        Task<SignInResponse> SignInWithExternalTokenAsync(ExternalTokenRequest externalToken);
        Task<SignInResponse> LinkWithExternalTokenAsync(string accessToken, ExternalTokenRequest externalToken);
        Task<SignInResponse> UnlinkExternalTokenAsync(string accessToken, UnlinkRequest request);
        Task DeleteAccountAsync(string accessToken, string playerId);
        Task<UserInfo> GetUserInfoAsync(string accessToken, string user);
    }

    class AuthenticationNetworkClient : IAuthenticationNetworkClient
    {
        const string k_WellKnownUrlStem = "/.well-known/jwks.json";
        const string k_AnonymousUrlStem = "/authentication/anonymous";
        const string k_SessionTokenUrlStem = "/authentication/session-token";
        const string k_ExternalTokenUrlStem = "/authentication/external-token";
        const string k_LinkExternalTokenUrlStem = "/authentication/link";
        const string k_UnlinkExternalTokenUrlStem = "/authentication/unlink";
        const string k_OAuthUrlStem = "/oauth2/auth";
        const string k_OAuthTokenUrlStem = "/oauth2/token";
        const string k_OauthRevokeStem = "/oauth2/revoke";
        const string k_UsersUrlStem = "/users";

        const string k_OAuthScope = "openid offline unity.user identity.user";
        const string k_AuthResponseType = "code";
        const string k_ChallengeMethod = "S256";

        readonly INetworkingUtilities m_NetworkClient;
        readonly ICodeChallengeGenerator m_CodeChallengeGenerator;

        readonly string m_WellKnownUrl;
        readonly string m_AnonymousUrl;
        readonly string m_SessionTokenUrl;
        readonly string m_ExternalTokenUrl;
        readonly string m_LinkExternalTokenUrl;
        readonly string m_UnlinkExternalTokenUrl;
        readonly string m_OAuthUrl;
        readonly string m_OAuthTokenUrl;
        readonly string m_OAuthRevokeTokenUrl;
        readonly string m_UsersUrl;

        readonly Dictionary<string, string> m_CommonHeaders;

        string m_OAuthClientId;
        string m_SessionChallengeCode;

        /// <summary>
        /// the environments component in the core registry.
        /// this is stored in case there is a reinitialization or a change in environments ever happens during runtime.
        /// </summary>
        IEnvironments m_EnvironmentComponent;

        internal AuthenticationNetworkClient(string host,
                                             string projectId,
                                             IEnvironments environment,
                                             ICodeChallengeGenerator codeChallengeGenerator,
                                             INetworkingUtilities networkClient)
        {
            m_NetworkClient = networkClient;
            m_CodeChallengeGenerator = codeChallengeGenerator;

            m_OAuthClientId = "default";

            m_WellKnownUrl = host + k_WellKnownUrlStem;
            m_AnonymousUrl = host + k_AnonymousUrlStem;
            m_SessionTokenUrl = host + k_SessionTokenUrlStem;
            m_ExternalTokenUrl = host + k_ExternalTokenUrlStem;
            m_LinkExternalTokenUrl = host + k_LinkExternalTokenUrlStem;
            m_UnlinkExternalTokenUrl = host + k_UnlinkExternalTokenUrlStem;
            m_OAuthUrl = host + k_OAuthUrlStem;
            m_OAuthTokenUrl = host + k_OAuthTokenUrlStem;
            m_OAuthRevokeTokenUrl = host + k_OauthRevokeStem;
            m_UsersUrl = host + k_UsersUrlStem;

            m_EnvironmentComponent = environment;

            m_CommonHeaders = new Dictionary<string, string>
            {
                ["ProjectId"] = projectId,
                // The Error-Version header enables RFC7807HttpError error responses
                ["Error-Version"] = "v1"
            };
        }

        public Task<WellKnownKeys> GetWellKnownKeysAsync()
        {
            return m_NetworkClient.GetAsync<WellKnownKeys>(m_WellKnownUrl);
        }

        public void SetOAuthClient(string oAuthClientId)
        {
            m_OAuthClientId = oAuthClientId;
        }

        public Task<SignInResponse> SignInAnonymouslyAsync()
        {
            return m_NetworkClient.PostAsync<SignInResponse>(m_AnonymousUrl, WithEnvironment(GetCommonHeaders()));
        }

        public Task<SignInResponse> SignInWithSessionTokenAsync(string token)
        {
            return m_NetworkClient.PostJsonAsync<SignInResponse>(m_SessionTokenUrl, new SessionTokenRequest
            {
                SessionToken = token
            }, WithEnvironment(GetCommonHeaders()));
        }

        public Task<SignInResponse> SignInWithExternalTokenAsync(ExternalTokenRequest externalToken)
        {
            return m_NetworkClient.PostJsonAsync<SignInResponse>(m_ExternalTokenUrl, externalToken, WithEnvironment(GetCommonHeaders()));
        }

        public Task<SignInResponse> LinkWithExternalTokenAsync(string accessToken, ExternalTokenRequest externalToken)
        {
            return m_NetworkClient.PostJsonAsync<SignInResponse>(m_LinkExternalTokenUrl, externalToken, WithEnvironment(WithAccessToken(GetCommonHeaders(), accessToken)));
        }

        public Task<SignInResponse> UnlinkExternalTokenAsync(string accessToken, UnlinkRequest request)
        {
            return m_NetworkClient.PostJsonAsync<SignInResponse>(m_UnlinkExternalTokenUrl, request, WithEnvironment(WithAccessToken(GetCommonHeaders(), accessToken)));
        }

        public Task DeleteAccountAsync(string accessToken, string playerId)
        {
            return m_NetworkClient.DeleteAsync(CreateUserRequestUrl(playerId), WithEnvironment(WithAccessToken(GetCommonHeaders(), accessToken)));
        }

        public Task<UserInfo> GetUserInfoAsync(string accessToken, string user)
        {
            return m_NetworkClient.GetAsync<UserInfo>(CreateUserRequestUrl(user), WithAccessToken(GetCommonHeaders(), accessToken));
        }

        string CreateUserRequestUrl(string user)
        {
            return $"{m_UsersUrl}/{user}";
        }

        public Task<OAuthAuthCodeResponse> RequestAuthCodeAsync(string idToken)
        {
            m_SessionChallengeCode = m_CodeChallengeGenerator.GenerateCode();

            var payload = $"client_id={m_OAuthClientId}&" +
                $"response_type={k_AuthResponseType}&" +
                $"id_token={idToken}&" +
                $"state={m_CodeChallengeGenerator.GenerateStateString()}&" +
                $"scope={k_OAuthScope}&" +
                $"code_challenge={S256EncodeChallenge(m_SessionChallengeCode)}&" +
                $"code_challenge_method={k_ChallengeMethod}";

            return m_NetworkClient.PostFormAsync<OAuthAuthCodeResponse>(m_OAuthUrl, payload, m_CommonHeaders);
        }

        string S256EncodeChallenge(string code)
        {
            using (var sha256 = SHA256.Create())
            {
                var codeVerifierBytes = Encoding.UTF8.GetBytes(code);
                var codeVerifierHash = sha256.ComputeHash(codeVerifierBytes);
                return UrlSafeBase64Encode(codeVerifierHash);
            }
        }

        string UrlSafeBase64Encode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }

        public Task<OAuthTokenResponse> RequestOAuthTokenAsync(string authCode)
        {
            var payload = $"client_id={m_OAuthClientId}" +
                "&grant_type=authorization_code" +
                $"&code_verifier={m_SessionChallengeCode}" +
                $"&code={authCode}";

            return m_NetworkClient.PostFormAsync<OAuthTokenResponse>(m_OAuthTokenUrl, payload, m_CommonHeaders);
        }

        public Task<OAuthTokenResponse> RevokeOAuthTokenAsync(string accessToken)
        {
            var payload = $"client_id={m_OAuthClientId}&token={accessToken}";

            return m_NetworkClient.PostFormAsync<OAuthTokenResponse>(m_OAuthRevokeTokenUrl, payload, m_CommonHeaders);
        }

        Dictionary<string, string> WithAccessToken(Dictionary<string, string> headers, string accessToken)
        {
            headers["Authorization"] = "Bearer " + accessToken;
            return headers;
        }

        Dictionary<string, string> WithEnvironment(Dictionary<string, string> headers)
        {
            var env = m_EnvironmentComponent.Current;
            if (!string.IsNullOrEmpty(env))
            {
                headers["UnityEnvironment"] = m_EnvironmentComponent.Current;
            }
            return headers;
        }

        Dictionary<string, string> GetCommonHeaders()
        {
            return new Dictionary<string, string>(m_CommonHeaders);
        }
    }
}
