using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Core;
using Unity.Services.Core.Configuration.Internal;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts
{
    class PlayerAccountServiceInternal : IPlayerAccountService
    {
        public event Action SignedIn;
        public event Action SignedOut;
        public event Action<RequestFailedException> SignInFailed;

        const string k_AccountPortalUrl = "https://player-account.unity.com";
        const string k_AuthUrl = "https://player-login.unity.com/v1/oauth2/auth";
        const string k_TokenUrl = "https://player-login.unity.com/v1/oauth2/token";
        const string k_CodeChallengeMethod = "S256";

        public string AccountPortalUrl => k_AccountPortalUrl;
        public bool IsSignedIn => SignInState == PlayerAccountState.Authorized || SignInState == PlayerAccountState.Refreshing;
        public string AccessToken { get; internal set; }
        public string IdToken { get; internal set; }
        public string RefreshToken { get; internal set; }
        public IdToken IdTokenClaims { get; internal set; }

        internal PlayerAccountState SignInState { get; set; }
        internal string RedirectUri { get; set; }
        internal string CodeVerifier { get; set; }
        internal string ClientId => m_Settings?.ClientId;

        readonly ICloudProjectId m_CloudProjectId;
        readonly IBrowserUtils m_BrowserUtils;
        readonly IJwtDecoder m_JwtDecoder;
        readonly INetworkHandler m_NetworkingClient;
        readonly UnityPlayerAccountSettings m_Settings;

        internal PlayerAccountServiceInternal(
            UnityPlayerAccountSettings settings,
            ICloudProjectId cloudProjectId,
            IJwtDecoder jwtDecoder,
            INetworkHandler networkingClient)
        {
            m_Settings = settings;
            m_CloudProjectId = cloudProjectId;
            m_BrowserUtils = BrowserUtils.CreateBrowserUtils(m_CloudProjectId, m_Settings, OnAuthCodeReceived);
            m_JwtDecoder = jwtDecoder;
            m_NetworkingClient = networkingClient;

            SignInState = PlayerAccountState.SignedOut;

            Application.deepLinkActivated += OnDeepLinkActivated;
        }

        public async Task StartSignInAsync(bool isSigningUp = false)
        {
            if (SignInState == PlayerAccountState.Authorized || SignInState == PlayerAccountState.Refreshing)
            {
                throw PlayerAccountsException.Create(PlayerAccountsErrorCodes.InvalidState, "Player is already signed in.");
            }

            if (string.IsNullOrEmpty(ClientId))
            {
                throw PlayerAccountsException.Create(PlayerAccountsErrorCodes.MissingClientId, "The Client Id is not configured.");
            }

            SignInState = PlayerAccountState.SigningIn;

            try
            {
                if (!m_BrowserUtils.Bind())
                {
                    throw PlayerAccountsException.Create(PlayerAccountsErrorCodes.InvalidState, "Platform binding failed");
                }

                await m_BrowserUtils.LaunchUrlAsync(BuildAuthorizationRequestUrl(isSigningUp));
            }
            catch (PlayerAccountsException exception)
            {
                SendSignInFailedEvent(exception, true);
                throw;
            }
            catch (RequestFailedException exception)
            {
                SendSignInFailedEvent(new RequestFailedException(exception.ErrorCode,
                    "Error opening system browser for OAuth 2.0 authorization request."
                    ), true);
            }
        }

        public Task RefreshTokenAsync()
        {
            if (!IsSignedIn)
            {
                throw PlayerAccountsException.Create(PlayerAccountsErrorCodes.InvalidState, "Player is not signed in.");
            }

            var refreshToken = RefreshToken;

            if (string.IsNullOrEmpty(refreshToken))
            {
                throw PlayerAccountsException.Create(PlayerAccountsErrorCodes.MissingRefreshToken, "Refresh token is null or empty.");
            }

            SignInState = PlayerAccountState.Refreshing;
            var refreshRequest =
                $"client_id={ClientId}&" +
                $"refresh_token={refreshToken}&" +
                $"grant_type=refresh_token";

            if (!string.IsNullOrEmpty(m_Settings.Scope))
            {
                refreshRequest += $"&scope={m_Settings.Scope}";
            }

            return HandleSignInRequestAsync(() => m_NetworkingClient.PostAsync<SignInResponse>(k_TokenUrl, refreshRequest));
        }

        public void SignOut()
        {
            AccessToken = null;
            var oldState = SignInState;
            SignInState = PlayerAccountState.SignedOut;

            if (oldState != PlayerAccountState.SigningIn)
            {
                SignedOut?.Invoke();
            }
        }

        string BuildAuthorizationRequestUrl(bool isSigningUp)
        {
            // Generate OAuth 2.0 request parameters
            var challengeGenerator = new CodeChallengeGenerator();
            CodeVerifier = challengeGenerator.GenerateCode();
            var state = challengeGenerator.GenerateStateString();
            var codeChallenge = CodeChallengeGenerator.S256EncodeChallenge(CodeVerifier);

            RedirectUri = m_BrowserUtils?.GetRedirectUri();

            // Create OAuth 2.0 authorization request.
            var authorizationRequest =
                $"{k_AuthUrl}?response_type=code&" +
                $"redirect_uri={Uri.EscapeDataString(RedirectUri)}&" +
                "response_mode=query&" +
                $"client_id={ClientId}&" +
                $"state={state}&" +
                $"code_challenge={codeChallenge}&" +
                $"code_challenge_method={k_CodeChallengeMethod}";

            if (isSigningUp)
            {
                authorizationRequest += "&action=sign-up";
            }

            if (!string.IsNullOrEmpty(m_Settings.Scope))
            {
                authorizationRequest += $"&scope={m_Settings.Scope}";
            }

            Logger.Log($"AuthorizationRequest URL: {authorizationRequest}");

            return authorizationRequest;
        }

        void OnDeepLinkActivated(string url)
        {
            var uri = new Uri(url);

            if (uri.Scheme != m_Settings.DeepLinkUriScheme)
            {
                return;
            }

            var queryParameters = UriHelper.ParseQueryString(uri.Query.Trim());
            var fragmentParameters = UriHelper.ParseQueryString(uri.Fragment.Trim());

            queryParameters.TryGetValue("code", out var code);
            queryParameters.TryGetValue("error", out var error);

            if (string.IsNullOrEmpty(code))
            {
                fragmentParameters.TryGetValue("code", out code);
            }

            if (string.IsNullOrEmpty(error))
            {
                fragmentParameters.TryGetValue("error", out error);
            }

            if (!string.IsNullOrEmpty(error))
            {
                throw PlayerAccountsExceptionHandler.HandleError(error);
            }

#if UNITY_IOS
            m_BrowserUtils.Dismiss();
#endif
            OnAuthCodeReceived(code);
        }

        void OnAuthCodeReceived(string code)
        {
            SignInRequestAsync(code, CodeVerifier, RedirectUri);
        }

        Task SignInRequestAsync(string code, string codeVerifier, string redirectUri)
        {
            var signInRequestBody =
                $"code={code}&" +
                $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                $"client_id={ClientId}&" +
                $"code_verifier={codeVerifier}&" +
                $"grant_type=authorization_code";

            return HandleSignInRequestAsync(() => m_NetworkingClient.PostAsync<SignInResponse>(k_TokenUrl, signInRequestBody));
        }

        async Task HandleSignInRequestAsync(Func<Task<SignInResponse>> signInRequest)
        {
            try
            {
                SignInState = PlayerAccountState.SigningIn;
                var response = await signInRequest();
                CompleteSignIn(response);
            }
            catch (RequestFailedException exception)
            {
                SendSignInFailedEvent(exception, true);
                throw;
            }
            catch (WebRequestException exception)
            {
                var errorResponse = JsonConvert.DeserializeObject<PlayerAccountsErrorResponse>(exception.Message);
                var playerAccountsException = PlayerAccountsExceptionHandler.HandleError(errorResponse?.Error, errorResponse?.Description, exception);

                Logger.LogException(playerAccountsException);
                throw playerAccountsException;
            }
        }

        void SendSignInFailedEvent(RequestFailedException exception, bool forceSignOut)
        {
            SignInFailed?.Invoke(exception);
            if (forceSignOut)
            {
                SignOut();
            }
        }

        internal void CompleteSignIn(SignInResponse signInResponse)
        {
            AccessToken = signInResponse?.AccessToken;
            IdToken = signInResponse?.IdToken;

            if (IdToken != null)
            {
                IdTokenClaims = m_JwtDecoder.Decode<IdToken>(IdToken);
            }

            RefreshToken = signInResponse?.RefreshToken;
            SignInState = PlayerAccountState.Authorized;
            SignedIn?.Invoke();
        }
    }
}
