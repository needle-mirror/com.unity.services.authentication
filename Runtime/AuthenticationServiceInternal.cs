using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Authentication.Models;
using Unity.Services.Authentication.Utilities;
using Unity.Services.Core;
using Unity.Services.Core.Scheduler.Internal;
using UnityEngine;
using Logger = Unity.Services.Authentication.Utilities.Logger;

namespace Unity.Services.Authentication
{
    class AuthenticationServiceInternal : IAuthenticationService
    {
        const string k_CacheKeySessionToken = "session_token";

        const string k_IdProviderApple = "apple.com";
        const string k_IdProviderFacebook = "facebook.com";
        const string k_IdProviderGoogle = "google.com";
        const string k_IdProviderSteam = "steampowered.com";
        const string k_ProfileRegex = @"^[a-zA-Z0-9_-]{1,30}$";

        public event Action<RequestFailedException> SignInFailed;
        public event Action SignedIn;
        public event Action SignedOut;
        public event Action Expired;

        public bool IsSignedIn =>
            State == AuthenticationState.Authorized ||
            State == AuthenticationState.Refreshing ||
            State == AuthenticationState.Expired;

        public bool IsAuthorized =>
            State == AuthenticationState.Authorized ||
            State == AuthenticationState.Refreshing;

        public bool IsExpired => State == AuthenticationState.Expired;

        public bool SessionTokenExists => !string.IsNullOrEmpty(ReadSessionToken());

        public string Profile => m_Profile.Current;
        public string AccessToken { get; private set; }

        public string PlayerId { get; internal set; }
        public UserInfo UserInfo { get; internal set; }

        internal DateTime AccessTokenExpiryTime { get; set; }
        internal long? ExpirationActionId { get; set; }
        internal long? RefreshActionId { get; set; }

        internal AuthenticationState State { get; set; }
        internal WellKnownKeys WellKnownKeys { get; set; }

        readonly IAuthenticationSettings m_Settings;
        readonly IAuthenticationNetworkClient m_NetworkClient;
        readonly IProfile m_Profile;
        readonly IJwtDecoder m_JwtDecoder;
        readonly IAuthenticationCache m_Cache;

        readonly IActionScheduler m_Scheduler;
        readonly IDateTimeWrapper m_DateTime;

        internal event Action<AuthenticationState, AuthenticationState> StateChanged;

        internal AuthenticationServiceInternal(IAuthenticationSettings settings,
                                               IAuthenticationNetworkClient networkClient,
                                               IProfile profile,
                                               IJwtDecoder jwtDecoder,
                                               IAuthenticationCache cache,
                                               IActionScheduler scheduler,
                                               IDateTimeWrapper dateTime)
        {
            m_Settings = settings;
            m_NetworkClient = networkClient;
            m_Profile = profile;
            m_JwtDecoder = jwtDecoder;
            m_Cache = cache;
            m_Scheduler = scheduler;
            m_DateTime = dateTime;

            State = AuthenticationState.SignedOut;
            MigrateCache();
        }

        public Task SignInAnonymouslyAsync()
        {
            if (State == AuthenticationState.SignedOut || State == AuthenticationState.Expired)
            {
                if (SessionTokenExists)
                {
                    return SignInWithSessionTokenAsync();
                }

                // I am a first-time anonymous user.
                return StartSigningInAsync(m_NetworkClient.SignInAnonymouslyAsync());
            }
            else
            {
                return Task.FromException(BuildClientInvalidStateException());
            }
        }

        public Task SignInWithSessionTokenAsync()
        {
            if (State == AuthenticationState.SignedOut || State == AuthenticationState.Expired)
            {
                var sessionToken = ReadSessionToken();

                if (string.IsNullOrEmpty(sessionToken))
                {
                    return Task.FromException(BuildClientSessionTokenNotExistsException());
                }

                Logger.LogVerbose("Continuing existing session with cached token.");

                return StartSigningInAsync(m_NetworkClient.SignInWithSessionTokenAsync(sessionToken));
            }
            else
            {
                return Task.FromException(BuildClientInvalidStateException());
            }
        }

        public Task SignInWithAppleAsync(string idToken)
        {
            return SignInWithExternalTokenAsync(new ExternalTokenRequest
            {
                IdProvider = k_IdProviderApple,
                Token = idToken
            });
        }

        public Task LinkWithAppleAsync(string idToken)
        {
            return LinkWithExternalTokenAsync(new ExternalTokenRequest
            {
                IdProvider = k_IdProviderApple,
                Token = idToken
            });
        }

        public Task UnlinkAppleAsync()
        {
            return UnlinkExternalTokenAsync(k_IdProviderApple);
        }

        public Task SignInWithGoogleAsync(string idToken)
        {
            return SignInWithExternalTokenAsync(new ExternalTokenRequest
            {
                IdProvider = k_IdProviderGoogle,
                Token = idToken
            });
        }

        public Task LinkWithGoogleAsync(string idToken)
        {
            return LinkWithExternalTokenAsync(new ExternalTokenRequest
            {
                IdProvider = k_IdProviderGoogle,
                Token = idToken
            });
        }

        public Task UnlinkGoogleAsync()
        {
            return UnlinkExternalTokenAsync(k_IdProviderGoogle);
        }

        public Task SignInWithFacebookAsync(string accessToken)
        {
            return SignInWithExternalTokenAsync(new ExternalTokenRequest
            {
                IdProvider = k_IdProviderFacebook,
                Token = accessToken
            });
        }

        public Task LinkWithFacebookAsync(string accessToken)
        {
            return LinkWithExternalTokenAsync(new ExternalTokenRequest
            {
                IdProvider = k_IdProviderFacebook,
                Token = accessToken
            });
        }

        public Task UnlinkFacebookAsync()
        {
            return UnlinkExternalTokenAsync(k_IdProviderFacebook);
        }

        public Task SignInWithSteamAsync(string sessionTicket)
        {
            return SignInWithExternalTokenAsync(new ExternalTokenRequest
            {
                IdProvider = k_IdProviderSteam,
                Token = sessionTicket
            });
        }

        public Task LinkWithSteamAsync(string sessionTicket)
        {
            return LinkWithExternalTokenAsync(new ExternalTokenRequest
            {
                IdProvider = k_IdProviderSteam,
                Token = sessionTicket
            });
        }

        public Task UnlinkSteamAsync()
        {
            return UnlinkExternalTokenAsync(k_IdProviderSteam);
        }

        public Task SignInWithExternalTokenAsync(ExternalTokenRequest externalToken)
        {
            if (State == AuthenticationState.SignedOut || State == AuthenticationState.Expired)
            {
                return StartSigningInAsync(m_NetworkClient.SignInWithExternalTokenAsync(externalToken));
            }
            else
            {
                return Task.FromException(BuildClientInvalidStateException());
            }
        }

        public async Task LinkWithExternalTokenAsync(ExternalTokenRequest request)
        {
            if (IsAuthorized)
            {
                try
                {
                    var response = await m_NetworkClient.LinkWithExternalTokenAsync(AccessToken, request);
                    UserInfo?.AddExternalId(response.UserInfo?.ExternalIds?.FirstOrDefault(x => x.ProviderId == request.IdProvider));
                }
                catch (WebRequestException e)
                {
                    throw BuildServerException(e);
                }
            }
            else
            {
                throw BuildClientInvalidStateException();
            }
        }

        public async Task UnlinkExternalTokenAsync(string providerId)
        {
            if (IsAuthorized)
            {
                var externalId = UserInfo?.GetExternalId(providerId);

                if (externalId == null)
                {
                    throw BuildClientUnlinkExternalIdNotFoundException();
                }

                try
                {
                    await m_NetworkClient.UnlinkExternalTokenAsync(AccessToken, new UnlinkRequest
                    {
                        IdProvider = providerId,
                        ExternalId = externalId
                    });

                    UserInfo.RemoveExternalId(providerId);
                }
                catch (WebRequestException e)
                {
                    throw BuildServerException(e);
                }
            }
            else
            {
                throw BuildClientInvalidStateException();
            }
        }

        public async Task DeleteAccountAsync()
        {
            if (IsAuthorized)
            {
                try
                {
                    await m_NetworkClient.DeleteAccountAsync(AccessToken, PlayerId);
                    SignOut();
                    ClearSessionToken();
                }
                catch (WebRequestException e)
                {
                    throw BuildServerException(e);
                }
            }
            else
            {
                throw BuildClientInvalidStateException();
            }
        }

        public void SignOut()
        {
            if (State != AuthenticationState.SignedOut)
            {
                AccessToken = null;
                AccessTokenExpiryTime = default;
                PlayerId = null;
                UserInfo = null;
                CancelScheduledRefresh();
                CancelScheduledExpiration();
                ChangeState(AuthenticationState.SignedOut);
            }
        }

        public void SwitchProfile(string profile)
        {
            if (State == AuthenticationState.SignedOut)
            {
                if (Regex.Match(profile, k_ProfileRegex).Success)
                {
                    m_Profile.Current = profile;
                }
                else
                {
                    throw BuildClientInvalidProfileException();
                }
            }
            else
            {
                throw BuildClientInvalidStateException();
            }
        }

        public void ClearSessionToken()
        {
            if (State == AuthenticationState.SignedOut)
            {
                if (SessionTokenExists)
                {
                    m_Cache.DeleteKey(k_CacheKeySessionToken);
                }
            }
            else
            {
                throw BuildClientInvalidStateException();
            }
        }

        public async Task<UserInfo> GetUserInfoAsync()
        {
            if (IsAuthorized)
            {
                try
                {
                    UserInfo = await m_NetworkClient.GetUserInfoAsync(AccessToken, PlayerId);
                    return UserInfo;
                }
                catch (WebRequestException e)
                {
                    throw BuildServerException(e);
                }
            }
            else
            {
                throw BuildClientInvalidStateException();
            }
        }

        internal async Task GetWellKnownKeysAsync(int attempt = 0)
        {
            while (attempt < m_Settings.WellKnownKeysMaxAttempt)
            {
                try
                {
                    WellKnownKeys = await m_NetworkClient.GetWellKnownKeysAsync();
                    return;
                }
                catch (WebRequestException e)
                {
                    Logger.LogWarning($"Well-known keys request failed (attempt: {attempt + 1}): {e.ResponseCode}, {e.Message}");

                    if (attempt < m_Settings.WellKnownKeysMaxAttempt - 1)
                    {
                        attempt++;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        internal Task RefreshAccessTokenAsync()
        {
            if (IsSignedIn)
            {
                if (State == AuthenticationState.Expired)
                {
                    return Task.CompletedTask;
                }

                var sessionToken = ReadSessionToken();

                if (string.IsNullOrEmpty(sessionToken))
                {
                    return Task.CompletedTask;
                }

                return StartRefreshAsync(sessionToken);
            }

            return Task.CompletedTask;
        }

        internal string ReadSessionToken()
        {
            return m_Cache.GetString(k_CacheKeySessionToken) ?? string.Empty;
        }

        internal async Task StartSigningInAsync(Task<SignInResponse> signInRequest)
        {
            try
            {
                ChangeState(AuthenticationState.SigningIn);
                await Task.WhenAll(signInRequest, WellKnownKeys == null? GetWellKnownKeysAsync() : Task.CompletedTask);
                CompleteSignIn(await signInRequest);
            }
            catch (WebRequestException e)
            {
                var authException = BuildServerException(e);
                SendSignInFailedEvent(authException);
                throw authException;
            }
        }

        internal async Task StartRefreshAsync(string sessionToken)
        {
            ChangeState(AuthenticationState.Refreshing);

            try
            {
                var response = await m_NetworkClient.SignInWithSessionTokenAsync(sessionToken);
                CompleteSignIn(response);
            }
            catch (RequestFailedException)
            {
                // Refresh failed since we received a bad token. Retry.
                Logger.LogWarning("The access token is not valid. Retry JWKS and refresh again.");

                if (State != AuthenticationState.Expired)
                {
                    Expire();
                }
            }
            catch (WebRequestException)
            {
                if (State == AuthenticationState.Refreshing)
                {
                    Logger.LogWarning("Failed to refresh access token due to network error or internal server error, will retry later.");
                    ChangeState(AuthenticationState.Authorized);
                    ScheduleRefresh(m_Settings.RefreshAttemptFrequency);
                }
            }
        }

        internal void CompleteSignIn(SignInResponse response)
        {
            try
            {
                var accessTokenDecoded = m_JwtDecoder.Decode<AccessToken>(response.IdToken, WellKnownKeys);
                if (accessTokenDecoded == null)
                {
                    throw AuthenticationException.Create(CommonErrorCodes.InvalidToken, "Failed to decode and verify access token.");
                }
                else
                {
                    AccessToken = response.IdToken;
                    PlayerId = response.UserId;
                    UserInfo = response.UserInfo;
                    m_Cache.SetString(k_CacheKeySessionToken, response.SessionToken);

                    var refreshTime = response.ExpiresIn - m_Settings.AccessTokenRefreshBuffer;
                    var expiryTime = response.ExpiresIn - m_Settings.AccessTokenExpiryBuffer;

                    AccessTokenExpiryTime = m_DateTime.UtcNow.AddSeconds(expiryTime);

                    ScheduleRefresh(refreshTime);
                    ScheduleExpiration(expiryTime);
                    ChangeState(AuthenticationState.Authorized);
                }
            }
            catch (Exception e)
            {
                throw AuthenticationException.Create(CommonErrorCodes.Unknown, "Unknown error completing sign-in.", e);
            }
        }

        internal void ScheduleRefresh(double delay)
        {
            CancelScheduledRefresh();

            if (m_DateTime.UtcNow.AddSeconds(delay) < AccessTokenExpiryTime)
            {
                Logger.LogVerbose($"Scheduling refresh in {delay} seconds.");
                RefreshActionId = m_Scheduler.ScheduleAction(ExecuteScheduledRefresh, delay);
            }
        }

        internal void ScheduleExpiration(double delay)
        {
            Logger.LogVerbose($"Scheduling expiration in {delay} seconds.");
            CancelScheduledExpiration();
            ExpirationActionId = m_Scheduler.ScheduleAction(ExecuteScheduledExpiration, delay);
        }

        internal void ExecuteScheduledRefresh()
        {
            Logger.LogVerbose($"Executing scheduled refresh.");
            RefreshActionId = null;
            RefreshAccessTokenAsync();
        }

        internal void ExecuteScheduledExpiration()
        {
            Logger.LogVerbose($"Executing scheduled expiration.");
            ExpirationActionId = null;
            Expire();
        }

        internal void CancelScheduledRefresh()
        {
            if (RefreshActionId.HasValue)
            {
                m_Scheduler.CancelAction(RefreshActionId.Value);
                RefreshActionId = null;
            }
        }

        internal void CancelScheduledExpiration()
        {
            if (ExpirationActionId.HasValue)
            {
                m_Scheduler.CancelAction(ExpirationActionId.Value);
                ExpirationActionId = null;
            }
        }

        internal void Expire()
        {
            AccessToken = null;
            AccessTokenExpiryTime = default;
            CancelScheduledRefresh();
            CancelScheduledExpiration();
            ChangeState(AuthenticationState.Expired);
        }

        internal void MigrateCache()
        {
            try
            {
                m_Cache.Migrate(k_CacheKeySessionToken);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        void ChangeState(AuthenticationState newState)
        {
            if (State == newState)
                return;

            Logger.LogVerbose($"Moved from state [{State}] to [{newState}]");

            var oldState = State;
            State = newState;

            HandleStateChanged(oldState, newState);
        }

        void HandleStateChanged(AuthenticationState oldState, AuthenticationState newState)
        {
            StateChanged?.Invoke(oldState, newState);
            switch (newState)
            {
                case AuthenticationState.Authorized:
                    if (oldState != AuthenticationState.Refreshing)
                    {
                        SignedIn?.Invoke();
                    }

                    break;
                case AuthenticationState.SignedOut:
                    if (oldState != AuthenticationState.SigningIn)
                    {
                        SignedOut?.Invoke();
                    }
                    break;
                case AuthenticationState.Expired:
                    Expired?.Invoke();
                    break;
            }
        }

        void SendSignInFailedEvent(RequestFailedException exception)
        {
            SignInFailed?.Invoke(exception);
            SignOut();
        }

        /// <summary>
        /// Returns an exception with <c>ClientInvalidUserState</c> error
        /// when the user is in an invalid state.
        /// </summary>
        /// <returns>The exception that represents the error.</returns>
        RequestFailedException BuildClientInvalidStateException()
        {
            var errorMessage = string.Empty;

            switch (State)
            {
                case AuthenticationState.SignedOut:
                    errorMessage = "Invalid state for this operation. The player is signed out.";
                    break;
                case AuthenticationState.SigningIn:
                    errorMessage = "Invalid state for this operation. The player is already signing in.";
                    break;
                case AuthenticationState.Authorized:
                case AuthenticationState.Refreshing:
                    errorMessage = "Invalid state for this operation. The player is already signed in.";
                    break;
                case AuthenticationState.Expired:
                    errorMessage = "Invalid state for this operation. The player session has expired.";
                    break;
            }

            return AuthenticationException.Create(AuthenticationErrorCodes.ClientInvalidUserState, errorMessage);
        }

        /// <summary>
        /// Returns an exception with <c>n</c> error
        /// when trying to switch to an invalid profile.
        /// </summary>
        /// <returns>The exception that represents the error.</returns>
        RequestFailedException BuildClientInvalidProfileException()
        {
            return AuthenticationException.Create(AuthenticationErrorCodes.ClientInvalidProfile, "Invalid profile name. The profile may only contain alphanumeric values, '-', '_', and must be no longer than 30 characters.");
        }

        /// <summary>
        /// Returns an exception with <c>UnlinkExternalIdNotFound</c> error
        /// when the user is calling <c>Unlink*</c> method but there is no external id found for the provider.
        /// </summary>
        /// <returns>The exception that represents the error.</returns>
        RequestFailedException BuildClientUnlinkExternalIdNotFoundException()
        {
            return AuthenticationException.Create(AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound, "No external id was found to unlink from the provider. Use GetUserInfoAsync to load the linked external ids.");
        }

        /// <summary>
        /// Returns an exception with <c>ClientNoActiveSession</c> error
        /// when the user is calling <c>SignInWithSessionToken</c> methods while there is no session token stored.
        /// </summary>
        /// <returns>The exception that represents the error.</returns>
        RequestFailedException BuildClientSessionTokenNotExistsException()
        {
            // At this point, the contents of the cache are invalid, and we don't want future
            // SignInAnonymously or SignInWithSessionToken to read the current contents of the key.
            m_Cache.DeleteKey(k_CacheKeySessionToken);
            return AuthenticationException.Create(AuthenticationErrorCodes.ClientNoActiveSession, "There is no cached session token.");
        }

        /// <summary>
        /// Convert a web request exception to an authentication or request failed exception.
        /// </summary>
        /// <param name="exception">The web request exception to convert.</param>
        /// <returns>The converted exception.</returns>
        internal RequestFailedException BuildServerException(WebRequestException exception)
        {
            Logger.LogError($"Request failed: {exception.ResponseCode}, {exception.Message}");

            if (exception.NetworkError)
            {
                return AuthenticationException.Create(CommonErrorCodes.TransportError, $"Network Error: {exception.Message}", exception);
            }

            try
            {
                var errorResponse = JsonConvert.DeserializeObject<AuthenticationErrorResponse>(exception.Message);
                return AuthenticationException.Create(MapErrorCodes(errorResponse.Title), errorResponse.Detail, exception);
            }
            catch (JsonException e)
            {
                return AuthenticationException.Create(CommonErrorCodes.Unknown, "Failed to deserialize server response.", e);
            }
            catch (Exception)
            {
                return AuthenticationException.Create(CommonErrorCodes.Unknown, "Unknown error deserializing server response. ", exception);
            }
        }

        int MapErrorCodes(string serverErrorTitle)
        {
            switch (serverErrorTitle)
            {
                case "ENTITY_EXISTS":
                    // This is the only reason why ENTITY_EXISTS is returned so far.
                    // Include the request/API context in case it has a different meaning in the future.
                    return AuthenticationErrorCodes.AccountAlreadyLinked;
                case "LINKED_ACCOUNT_LIMIT_EXCEEDED":
                    return AuthenticationErrorCodes.AccountLinkLimitExceeded;
                case "INVALID_PARAMETERS":
                    return AuthenticationErrorCodes.InvalidParameters;
                case "PERMISSION_DENIED":
                    // This is the server side response when the third party token is invalid to sign-in or link a user.
                    // Also map to AuthenticationErrorCodes.InvalidParameters since it's basically an invalid parameter.
                    // Include the request/API context in case it has a different meaning in the future.
                    return AuthenticationErrorCodes.InvalidParameters;
                case "UNAUTHORIZED_REQUEST":
                    // This happens when either the token is invalid or the token has expired.
                    return CommonErrorCodes.InvalidToken;
            }

            return CommonErrorCodes.Unknown;
        }
    }
}
