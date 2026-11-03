using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Device.Internal;
using Unity.Services.Core.Environments.Internal;
#if FEATURE_LIVE_RELEASES
using Unity.Services.Core.Releases.Internal;
#if UNITY_EDITOR
using Unity.Services.Authentication.Internal;
#endif
#endif

namespace Unity.Services.Authentication
{
    class AuthenticationNetworkClient : IAuthenticationNetworkClient
    {
        const string k_PlayerIdReplacement = "{PlayerId}";

        const string k_AnonymousUrlStem = "/v1/authentication/anonymous";
        const string k_SessionTokenUrlStem = "/v1/authentication/session-token";
#if FEATURE_LIVE_RELEASES
        const string k_AnonymousImpersonatedUrlStem = "/editor/v1/authentication/anonymous-impersonated";
        const string k_SessionTokenImpersonatedUrlStem = "/editor/v1/authentication/session-token-impersonated";
#endif
        const string k_ExternalTokenUrlStem = "/v1/authentication/external-token";
        const string k_LinkExternalTokenUrlStem = "/v1/authentication/link";
        const string k_UnlinkExternalTokenUrlStem = "/v1/authentication/unlink";
        const string k_UsersUrlStem = "/v1/users";
        const string k_UsernamePasswordSignInUrlStem = "/v1/authentication/usernamepassword/sign-in";
        const string k_UsernamePasswordSignUpUrlStem = "/v1/authentication/usernamepassword/sign-up";
        const string k_UpdatePasswordUrlStem = "/v1/authentication/usernamepassword/update-password";
        const string k_GenerateSignInCodeUrlStem = "/v1/authentication/code-link/generate";
        const string k_ConfirmSignInCodeUrlStem = "/v1/authentication/code-link/confirm";
        const string k_GetCodeIdentifierUrlStem = "/v1/authentication/code-link/info";
        const string k_CodeSignInUrlStem = "/v1/authentication/code-link/sign-in";
        const string k_GetNotificationsStem = "/v1/users/{PlayerId}/notifications";

        const string k_InstallationIdHeader = "Unity-Installation-Id";


        internal AccessTokenComponent AccessTokenComponent { get; }
        internal ICloudProjectId CloudProjectIdComponent { get; }
        internal IEngineInstallationId EngineInstallationIdComponent { get; }
        internal IEnvironments EnvironmentComponent { get; }
#if FEATURE_LIVE_RELEASES
        internal IReleases ReleaseComponent { get; }
#endif
        internal INetworkHandler NetworkHandler { get; }

        string AccessToken => AccessTokenComponent.AccessToken;
        string EnvironmentName => EnvironmentComponent.Current;
#if FEATURE_LIVE_RELEASES
        string ReleasePointer => ReleaseComponent?.Pointer;
        string ReleaseName => ReleaseComponent?.ReleaseName;
#endif

        readonly string m_AnonymousUrl;
        readonly string m_SessionTokenUrl;
#if FEATURE_LIVE_RELEASES
#if UNITY_EDITOR
        readonly string m_AnonymousImpersonatedUrl;
        readonly string m_SessionTokenImpersonatedUrl;
#endif
#endif
        readonly string m_ExternalTokenUrl;
        readonly string m_LinkExternalTokenUrl;
        readonly string m_UnlinkExternalTokenUrl;
        readonly string m_UsersUrl;
        readonly string m_UsernamePasswordSignInUrl;
        readonly string m_UsernamePasswordSignUpUrl;
        readonly string m_UpdatePasswordUrl;
        readonly string m_GenerateSignInCodeUrl;
        readonly string m_ConfirmSignInCodeUrl;
        readonly string m_CodeSignInUrl;
        readonly string m_GetCodeIdentifierUrl;
        readonly string m_GetNotificationsUrl;

        readonly Dictionary<string, string> m_CommonHeaders;


        internal AuthenticationNetworkClient(string host,
                                             ICloudProjectId cloudProjectId,
                                             IEnvironments environment,
                                             INetworkHandler networkHandler,
                                             AccessTokenComponent accessToken,
                                             IEngineInstallationId engineInstallationId
                                             #if FEATURE_LIVE_RELEASES
                                             , IReleases release = null
#endif
        )
        {
            AccessTokenComponent = accessToken;
            CloudProjectIdComponent = cloudProjectId;
            EngineInstallationIdComponent = engineInstallationId;
            EnvironmentComponent = environment;
#if FEATURE_LIVE_RELEASES
            ReleaseComponent = release;
#endif
            NetworkHandler = networkHandler;

            m_AnonymousUrl = host + k_AnonymousUrlStem;
            m_SessionTokenUrl = host + k_SessionTokenUrlStem;
#if FEATURE_LIVE_RELEASES
#if UNITY_EDITOR
            m_AnonymousImpersonatedUrl = host + k_AnonymousImpersonatedUrlStem;
            m_SessionTokenImpersonatedUrl = host + k_SessionTokenImpersonatedUrlStem;
#endif
#endif
            m_ExternalTokenUrl = host + k_ExternalTokenUrlStem;
            m_LinkExternalTokenUrl = host + k_LinkExternalTokenUrlStem;
            m_UnlinkExternalTokenUrl = host + k_UnlinkExternalTokenUrlStem;
            m_UsersUrl = host + k_UsersUrlStem;
            m_UsernamePasswordSignInUrl = host + k_UsernamePasswordSignInUrlStem;
            m_UsernamePasswordSignUpUrl = host + k_UsernamePasswordSignUpUrlStem;
            m_UpdatePasswordUrl = host + k_UpdatePasswordUrlStem;
            m_GenerateSignInCodeUrl = host + k_GenerateSignInCodeUrlStem;
            m_ConfirmSignInCodeUrl = host + k_ConfirmSignInCodeUrlStem;
            m_GetCodeIdentifierUrl = host + k_GetCodeIdentifierUrlStem;
            m_CodeSignInUrl = host + k_CodeSignInUrlStem;
            m_GetNotificationsUrl = host + k_GetNotificationsStem;

            m_CommonHeaders = new Dictionary<string, string>
            {
                ["ProjectId"] = CloudProjectIdComponent.GetCloudProjectId(),
                // The Error-Version header enables RFC7807HttpError error responses
                ["Error-Version"] = "v1"
            };
        }

        public async Task<SignInResponse> SignInAnonymouslyAsync(SignInAnonymouslyRequest request)
        {
            var headers = WithEnvironmentAndRelease(GetCommonHeaders());
#if FEATURE_LIVE_RELEASES
#if UNITY_EDITOR
            if (ShouldImpersonate())
            {
                var impersonatedHeaders = await TryBuildImpersonationHeadersAsync(headers);
                if (impersonatedHeaders != null)
                {
                    return await NetworkHandler.PostAsync<SignInResponse>(m_AnonymousImpersonatedUrl, request, impersonatedHeaders);
                }
            }
#endif
#endif
            return await NetworkHandler.PostAsync<SignInResponse>(m_AnonymousUrl, request, headers);
        }

        public async Task<SignInResponse> SignInWithSessionTokenAsync(SessionTokenRequest request)
        {
            var headers = WithEnvironmentAndRelease(GetCommonHeaders());
#if FEATURE_LIVE_RELEASES
#if UNITY_EDITOR
            if (ShouldImpersonate())
            {
                var impersonatedHeaders = await TryBuildImpersonationHeadersAsync(headers);
                if (impersonatedHeaders != null)
                {
                    return await NetworkHandler.PostAsync<SignInResponse>(m_SessionTokenImpersonatedUrl, request, impersonatedHeaders);
                }
            }
#endif
#endif
            return await NetworkHandler.PostAsync<SignInResponse>(m_SessionTokenUrl, request, headers);
        }

        public async Task<SignInResponse> GenerateRestrictedTokenAsync(SessionTokenRequest request)
        {
            // Same endpoint as refresh; distinct method so the response is returned to the caller
            // rather than updating the cached session.
            return await NetworkHandler.PostAsync<SignInResponse>(m_SessionTokenUrl, request, WithInstallationId(WithEnvironmentAndRelease(GetCommonHeaders())));
        }

        public async Task<SignInResponse> SignInWithExternalTokenAsync(string idProvider, SignInWithExternalTokenRequest externalToken)
        {
            var url = $"{m_ExternalTokenUrl}/{idProvider}";
            return await NetworkHandler.PostAsync<SignInResponse>(url, externalToken, WithEnvironmentAndRelease(GetCommonHeaders()));
        }

        public async Task<LinkResponse> LinkWithExternalTokenAsync(string idProvider, LinkWithExternalTokenRequest externalToken)
        {
            var url = $"{m_LinkExternalTokenUrl}/{idProvider}";
            return await NetworkHandler.PostAsync<LinkResponse>(url, externalToken, WithEnvironmentAndRelease(WithAccessToken(GetCommonHeaders())));
        }

        public async Task<UnlinkResponse> UnlinkExternalTokenAsync(string idProvider, UnlinkRequest request)
        {
            var url = $"{m_UnlinkExternalTokenUrl}/{idProvider}";
            return await NetworkHandler.PostAsync<UnlinkResponse>(url, request, WithEnvironmentAndRelease(WithAccessToken(GetCommonHeaders())));
        }

        public Task<PlayerInfoResponse> GetPlayerInfoAsync(string playerId)
        {
            return NetworkHandler.GetAsync<PlayerInfoResponse>(CreateUserRequestUrl(playerId), WithAccessToken(GetCommonHeaders()));
        }

        public async Task DeleteAccountAsync(string playerId)
        {
            await NetworkHandler.DeleteAsync(CreateUserRequestUrl(playerId), WithEnvironmentAndRelease(WithAccessToken(GetCommonHeaders())));
        }

        public async Task<SignInResponse> SignInWithUsernamePasswordAsync(UsernamePasswordRequest credentials)
        {
            return await NetworkHandler.PostAsync<SignInResponse>(m_UsernamePasswordSignInUrl, credentials, WithEnvironmentAndRelease(GetCommonHeaders()));
        }

        public async Task<SignInResponse> SignUpWithUsernamePasswordAsync(UsernamePasswordRequest credentials)
        {
            return await NetworkHandler.PostAsync<SignInResponse>(m_UsernamePasswordSignUpUrl, credentials, WithEnvironmentAndRelease(GetCommonHeaders()));
        }

        public async Task<SignInResponse> AddUsernamePasswordAsync(UsernamePasswordRequest credentials)
        {
            return await NetworkHandler.PostAsync<SignInResponse>(m_UsernamePasswordSignUpUrl, credentials, WithEnvironmentAndRelease(WithAccessToken(GetCommonHeaders())));
        }

        public async Task<SignInResponse> UpdatePasswordAsync(UpdatePasswordRequest credentials)
        {
            return await NetworkHandler.PostAsync<SignInResponse>(m_UpdatePasswordUrl, credentials, WithEnvironmentAndRelease(WithAccessToken(GetCommonHeaders())));
        }

        public async Task<GenerateCodeResponse> GenerateSignInCodeAsync(GenerateSignInCodeRequest request)
        {
            return await NetworkHandler.PostAsync<GenerateCodeResponse>(m_GenerateSignInCodeUrl, request, WithEnvironmentAndRelease(GetCommonHeaders()));
        }

        public async Task<CodeLinkConfirmResponse> ConfirmCodeAsync(ConfirmSignInCodeRequest request)
        {
            return await NetworkHandler.PostAsync<CodeLinkConfirmResponse>(m_ConfirmSignInCodeUrl, request, WithEnvironmentAndRelease(WithAccessToken(GetCommonHeaders())));
        }

        public async Task<SignInResponse> SignInWithCodeAsync(SignInWithCodeRequest request)
        {
            var url = $"{m_CodeSignInUrl}/{request.CodeLinkSessionId}";
            return await NetworkHandler.PostAsync<SignInResponse>(url, request, WithEnvironmentAndRelease(GetCommonHeaders()));
        }

        public async Task<CodeLinkInfoResponse> GetCodeIdentifierAsync(CodeLinkInfoRequest request)
        {
            return await NetworkHandler.PostAsync<CodeLinkInfoResponse>(m_GetCodeIdentifierUrl, request, WithEnvironmentAndRelease(WithAccessToken(GetCommonHeaders())));
        }

        public async Task<GetNotificationsResponse> GetNotificationsAsync(string playerId)
        {
            return await NetworkHandler.GetAsync<GetNotificationsResponse>(m_GetNotificationsUrl.Replace(k_PlayerIdReplacement, playerId), WithEnvironmentAndRelease(WithAccessToken(GetCommonHeaders())));
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

        Dictionary<string, string> WithInstallationId(Dictionary<string, string> headers)
        {
            // Backend only reads this on the session-token endpoint; omit when empty so we never
            // send a blank header.
            var installationId = EngineInstallationIdComponent?.GetOrCreateIdentifier();

            if (!string.IsNullOrEmpty(installationId))
            {
                headers[k_InstallationIdHeader] = installationId;
            }

            return headers;
        }

        Dictionary<string, string> WithEnvironmentAndRelease(Dictionary<string, string> headers)
        {
            var environmentName = EnvironmentName;

            if (!string.IsNullOrEmpty(environmentName))
            {
                headers["UnityEnvironment"] = environmentName;
            }

#if FEATURE_LIVE_RELEASES
            var releasePointer = ReleasePointer;

            if (!string.IsNullOrEmpty(releasePointer))
            {
                // Code-set pointer takes precedence over Project Settings release.
                headers["UnityLiveReleasePointer"] = releasePointer;
            }
#endif

            return headers;
        }

#if FEATURE_LIVE_RELEASES
#if UNITY_EDITOR
        bool ShouldImpersonate()
        {
            return !string.IsNullOrEmpty(ReleaseName) &&
                   string.IsNullOrEmpty(ReleasePointer) &&
                   PlayModeImpersonationBridge.GenesisTokenProvider != null;
        }

        async Task<Dictionary<string, string>> TryBuildImpersonationHeadersAsync(Dictionary<string, string> headers)
        {
            string genesisToken;
            try
            {
                genesisToken = await PlayModeImpersonationBridge.GenesisTokenProvider();
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Play mode impersonation could not obtain a gateway token; " +
                    $"signing in without release override. ({e.Message})");
                return null;
            }
            if (string.IsNullOrEmpty(genesisToken))
            {
                return null;
            }
            headers["Authorization"] = $"Bearer {genesisToken}";
            headers["unity-live-release-name"] = ReleaseName;
            return headers;
        }
#endif
#endif

        Dictionary<string, string> GetCommonHeaders()
        {
            return new Dictionary<string, string>(m_CommonHeaders);
        }
    }
}
