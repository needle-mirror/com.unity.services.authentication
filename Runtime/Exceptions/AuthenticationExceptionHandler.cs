using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using Unity.Services.Authentication.Shared;
using Unity.Services.Core;

namespace Unity.Services.Authentication
{
    internal class AuthenticationExceptionHandler : IAuthenticationExceptionHandler
    {
        IAuthenticationMetrics Metrics { get; }

        public AuthenticationExceptionHandler(IAuthenticationMetrics metrics)
        {
            Metrics = metrics;
        }

        /// <inheritdoc/>
        public RequestFailedException BuildClientInvalidStateException(AuthenticationState state)
        {
            var errorMessage = string.Empty;

            switch (state)
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

            Metrics.SendClientInvalidStateExceptionMetric();
            return AuthenticationException.Create(AuthenticationErrorCodes.ClientInvalidUserState, errorMessage);
        }

        /// <inheritdoc/>
        public RequestFailedException BuildClientInvalidProfileException()
        {
            return AuthenticationException.Create(AuthenticationErrorCodes.ClientInvalidProfile, "Invalid profile name. The profile may only contain alphanumeric values, '-', '_', and must be no longer than 30 characters.");
        }

        /// <inheritdoc/>
        public RequestFailedException BuildClientUnlinkExternalIdNotFoundException()
        {
            Metrics.SendUnlinkExternalIdNotFoundExceptionMetric();
            return AuthenticationException.Create(AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound, "No external id was found to unlink from the provider. Use GetPlayerInfoAsync to load the linked external ids.");
        }

        /// <inheritdoc/>
        public RequestFailedException BuildClientSessionTokenNotExistsException()
        {
            // At this point, the contents of the cache are invalid, and we don't want future
            Metrics.SendClientSessionTokenNotExistsExceptionMetric();
            return AuthenticationException.Create(AuthenticationErrorCodes.ClientNoActiveSession, "There is no cached session token.");
        }

        /// <inheritdoc/>
        public RequestFailedException BuildUnknownException(string error)
        {
            return AuthenticationException.Create(CommonErrorCodes.Unknown, error);
        }

        /// <inheritdoc/>
        public RequestFailedException BuildInvalidIdProviderNameException()
        {
            return AuthenticationException.Create(AuthenticationErrorCodes.InvalidParameters, "Invalid IdProviderName. The Id Provider name should start with 'oidc-' and have between 6 and 20 characters (including 'oidc-')");
        }

        /// <inheritdoc/>
        public RequestFailedException BuildInvalidPlayerNameException()
        {
            return AuthenticationException.Create(AuthenticationErrorCodes.InvalidParameters, "Invalid Player Name. Player names cannot be empty or contain spaces.");
        }

        /// <inheritdoc/>
        public RequestFailedException ConvertException(WebRequestException exception)
        {
            var errorLogBuilder = new StringBuilder();
            var errorLog = $"Request failed: {exception.ResponseCode}, {exception.Message}";
            errorLogBuilder.Append(errorLog);

            if (exception.ResponseHeaders != null)
            {
                if (exception.ResponseHeaders.TryGetValue("x-request-id", out var requestId))
                {
                    errorLogBuilder.Append($", request-id: {requestId}");
                }
            }

            Logger.Log(errorLogBuilder.ToString());

            if (exception.NetworkError)
            {
                Metrics.SendNetworkErrorMetric();
                return AuthenticationException.Create(CommonErrorCodes.TransportError, $"Network Error: {exception.Message}", exception);
            }

            try
            {
                var errorResponse = JsonConvert.DeserializeObject<AuthenticationErrorResponse>(exception.Message);
                var errorCode = MapErrorCodes(errorResponse.Title);

                return AuthenticationException.Create(errorCode, errorResponse.Detail, exception);
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

        /// <inheritdoc/>
        public RequestFailedException ConvertException(ApiException exception)
        {
            switch (exception?.Type)
            {
                case ApiExceptionType.InvalidParameters:
                    throw AuthenticationException.Create(AuthenticationErrorCodes.InvalidParameters, exception.Message);
                case ApiExceptionType.Deserialization:
                    return AuthenticationException.Create(CommonErrorCodes.Unknown, exception.Message);
                case ApiExceptionType.Network: // 5XX
                    switch (exception?.Response?.StatusCode)
                    {
                        case HttpStatusCode.ServiceUnavailable:
                            throw AuthenticationException.Create(CommonErrorCodes.ServiceUnavailable, exception.Message);
                        case HttpStatusCode.GatewayTimeout:
                            throw AuthenticationException.Create(CommonErrorCodes.Timeout, exception.Message);
                        default:
                            throw AuthenticationException.Create(CommonErrorCodes.TransportError, exception.Message);
                    }
                case ApiExceptionType.Http: // 4XX
                    switch (exception?.Response?.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                            throw AuthenticationException.Create(CommonErrorCodes.InvalidRequest, exception.Message);
                        case HttpStatusCode.Unauthorized:
                            throw AuthenticationException.Create(CommonErrorCodes.InvalidToken, exception.Message);
                        case HttpStatusCode.Forbidden:
                            throw AuthenticationException.Create(CommonErrorCodes.Forbidden, exception.Message);
                        case HttpStatusCode.NotFound:
                            throw AuthenticationException.Create(CommonErrorCodes.NotFound, exception.Message);
                        case HttpStatusCode.RequestTimeout:
                            throw AuthenticationException.Create(CommonErrorCodes.Timeout, exception.Message);
                        default:
                            throw AuthenticationException.Create(CommonErrorCodes.InvalidRequest, exception.Message);
                    }
            }

            return AuthenticationException.Create(CommonErrorCodes.TransportError, $"Network Error: {exception.Message}", exception);
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
                case "INVALID_SESSION_TOKEN":
                    return AuthenticationErrorCodes.InvalidSessionToken;
                case "PERMISSION_DENIED":
                    // This is the server side response when the third party token is invalid to sign-in or link a player.
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
