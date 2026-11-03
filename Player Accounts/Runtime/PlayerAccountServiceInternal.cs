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
        internal string OAuthState { get; set; }
        internal string ClientId => m_Settings?.ClientId;

        readonly ICloudProjectId m_CloudProjectId;
        readonly IBrowserUtils m_BrowserUtils;
        readonly IJwtDecoder m_JwtDecoder;
        readonly INetworkHandler m_NetworkingClient;
        readonly UnityPlayerAccountSettings m_Settings;

        // Bumped by every sign-in attempt and every sign-out, so work started by an abandoned attempt can tell it is stale.
        int m_SignInAttempt;

        // Concurrent RefreshTokenAsync calls share one request: refresh tokens may be single-use, so a second request could fail and sign out the session the first one refreshed.
        Task m_PendingRefresh;

        internal PlayerAccountServiceInternal(
            UnityPlayerAccountSettings settings,
            ICloudProjectId cloudProjectId,
            IJwtDecoder jwtDecoder,
            INetworkHandler networkingClient)
        {
            m_Settings = settings;
            m_CloudProjectId = cloudProjectId;
            m_BrowserUtils = BrowserUtils.CreateBrowserUtils(m_CloudProjectId, m_Settings, OnAuthCodeReceived, this);
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

            var attempt = ++m_SignInAttempt;
            SignInState = PlayerAccountState.SigningIn;

            try
            {
                if (!m_BrowserUtils.Bind())
                {
                    throw PlayerAccountsException.Create(PlayerAccountsErrorCodes.InvalidState, "Platform binding failed");
                }

                await m_BrowserUtils.LaunchUrlAsync(BuildAuthorizationRequestUrl(isSigningUp));
            }
            catch (RequestFailedException) when (attempt != m_SignInAttempt)
            {
                // The player signed out or restarted sign-in; this attempt's failure no longer matters.
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

            if (SignInState == PlayerAccountState.Refreshing && m_PendingRefresh != null)
            {
                return m_PendingRefresh;
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

            m_PendingRefresh = HandleSignInRequestAsync(() => m_NetworkingClient.PostAsync<SignInResponse>(k_TokenUrl, refreshRequest), true);
            return m_PendingRefresh;
        }

        public void SignOut()
        {
            m_SignInAttempt++;
            AccessToken = null;
            CodeVerifier = null;
            OAuthState = null;
            m_BrowserUtils?.Cancel();
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
            OAuthState = challengeGenerator.GenerateStateString();
            var codeChallenge = CodeChallengeGenerator.S256EncodeChallenge(CodeVerifier);

            RedirectUri = m_BrowserUtils?.GetRedirectUri();

            // Create OAuth 2.0 authorization request.
            var authorizationRequest =
                $"{k_AuthUrl}?response_type=code&" +
                $"redirect_uri={Uri.EscapeDataString(RedirectUri)}&" +
                "response_mode=query&" +
                $"client_id={ClientId}&" +
                $"state={OAuthState}&" +
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

        internal void OnDeepLinkActivated(string url)
        {
            var uri = new Uri(url);

            if (uri.Scheme != m_Settings.DeepLinkUriScheme)
            {
                return;
            }

            if (SignInState != PlayerAccountState.SigningIn)
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

            if (string.IsNullOrEmpty(code))
            {
                return;
            }

            if (!queryParameters.TryGetValue("state", out var state) || string.IsNullOrEmpty(state))
            {
                fragmentParameters.TryGetValue("state", out state);
            }

            if (string.IsNullOrEmpty(OAuthState) || state != OAuthState)
            {
                // Ignore rather than fail the sign-in: any app can send this scheme, so a forged callback must not
                // cancel the attempt or clear OAuthState while the legitimate callback may still arrive.
                // Standalone does the same: its loopback listener ignores mismatched callbacks and keeps waiting.
                Logger.LogWarning("Ignoring deep-link callback with invalid state.");
                return;
            }

            OAuthState = null;
            OnAuthCodeReceived(code);
        }

        internal void OnAuthCodeReceived(string code)
        {
            var codeVerifier = CodeVerifier;

            if (SignInState != PlayerAccountState.SigningIn || string.IsNullOrEmpty(codeVerifier))
            {
                Logger.Log("Ignoring authorization code: no sign-in in progress.");
                return;
            }

            // Codes are single-use, so a repeated deep link must not start a second exchange that fails and signs the player out.
            CodeVerifier = null;
            SignInRequestAsync(code, codeVerifier, RedirectUri);
        }

        Task SignInRequestAsync(string code, string codeVerifier, string redirectUri)
        {
            var signInRequestBody =
                $"code={code}&" +
                $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                $"client_id={ClientId}&" +
                $"code_verifier={codeVerifier}&" +
                $"grant_type=authorization_code";

            return HandleSignInRequestAsync(() => m_NetworkingClient.PostAsync<SignInResponse>(k_TokenUrl, signInRequestBody), false);
        }

        async Task HandleSignInRequestAsync(Func<Task<SignInResponse>> signInRequest, bool isRefresh)
        {
            var attempt = m_SignInAttempt;
            SignInResponse response = null;

            try
            {
                if (!isRefresh)
                {
                    SignInState = PlayerAccountState.SigningIn;
                }

                response = await signInRequest();
            }
            catch (Exception) when (attempt != m_SignInAttempt)
            {
                // The player signed out or restarted sign-in while the request was in flight; handled below.
            }
            catch (WebRequestException exception)
            {
                var playerAccountsException = ToPlayerAccountsException(exception);
                Logger.LogException(playerAccountsException);

                if (isRefresh)
                {
                    if (playerAccountsException.ErrorCode == PlayerAccountsErrorCodes.InvalidGrant)
                    {
                        // The refresh token is revoked or expired, so no retry can succeed.
                        SignOut();
                    }
                    else
                    {
                        // Transient failure: the tokens are still held, so the caller can retry.
                        SignInState = PlayerAccountState.Authorized;
                    }

                    throw playerAccountsException;
                }

                // The code exchange runs fire-and-forget, so the event is the only way a caller hears about it.
                SendSignInFailedEvent(playerAccountsException, true);
                return;
            }

            if (attempt != m_SignInAttempt)
            {
                Logger.Log("Discarding token request result: the player signed out while it was in flight.");

                if (isRefresh)
                {
                    throw PlayerAccountsException.Create(PlayerAccountsErrorCodes.InvalidState, "Token refresh was cancelled by a sign-out or a new sign-in.");
                }

                return;
            }

            CompleteSignIn(response);
        }

        static PlayerAccountsException ToPlayerAccountsException(WebRequestException exception)
        {
            PlayerAccountsErrorResponse errorResponse = null;

            try
            {
                errorResponse = JsonConvert.DeserializeObject<PlayerAccountsErrorResponse>(exception.Message);
            }
            catch (JsonException)
            {
                // Network errors, proxy HTML pages and deserialization failures aren't Player Accounts error bodies.
            }

            if (string.IsNullOrEmpty(errorResponse?.Error))
            {
                // A server error body here is likely a proxy page; the raw body stays on the inner exception.
                var message = exception.ServerError ? $"Token request failed (HTTP {exception.ResponseCode})." : exception.Message;
                return PlayerAccountsException.Create(PlayerAccountsErrorCodes.UnknownError, message, exception);
            }

            return PlayerAccountsExceptionHandler.HandleError(errorResponse.Error, errorResponse.Description, exception);
        }

        void SendSignInFailedEvent(RequestFailedException exception, bool forceSignOut)
        {
            // Sign out first so a retry started from a SignInFailed handler isn't torn down afterwards.
            if (forceSignOut)
            {
                SignOut();
            }

            SignInFailed?.Invoke(exception);
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
