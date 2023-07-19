using Newtonsoft.Json;
using System;
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
        public RequestFailedException BuildInvalidCredentialsException()
        {
            return AuthenticationException.Create(AuthenticationErrorCodes.InvalidParameters, "Username and/or Password are not in the correct format");
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
                var errorResponse = IsolatedJsonConvert.DeserializeObject<AuthenticationErrorResponse>(exception.Message, SerializerSettings.DefaultSerializerSettings);
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
                    return AuthenticationException.Create(AuthenticationErrorCodes.InvalidParameters, exception.Message);
                case ApiExceptionType.Deserialization:
                    return AuthenticationException.Create(CommonErrorCodes.Unknown, exception.Message);
                case ApiExceptionType.Network: // 5XX
                    return CreateNetworkException(exception);
                case ApiExceptionType.Http: // 4XX
                    return CreateHttpException(exception);
                default:
                    return CreateUnknownException(exception);
            }
        }

        static RequestFailedException CreateNetworkException(ApiException exception)
        {
            switch (exception?.Response?.StatusCode)
            {
                case 503: // HttpStatusCode.ServiceUnavailable
                    return AuthenticationException.Create(CommonErrorCodes.ServiceUnavailable, exception.Message);
                case 504: // HttpStatusCode.GatewayTimeout
                    return AuthenticationException.Create(CommonErrorCodes.Timeout, exception.Message);
                default:
                    return AuthenticationException.Create(CommonErrorCodes.TransportError, exception.Message);
            }
        }

        static RequestFailedException CreateHttpException(ApiException exception)
        {
            switch (exception?.Response?.StatusCode)
            {
                case 400: // HttpStatusCode.BadRequest
                    return AuthenticationException.Create(CommonErrorCodes.InvalidRequest, exception.Message);
                case 401: // HttpStatusCode.Unauthorized
                    return AuthenticationException.Create(CommonErrorCodes.InvalidToken, exception.Message);
                case 403: // HttpStatusCode.Forbidden
                    return AuthenticationException.Create(CommonErrorCodes.Forbidden, exception.Message);
                case 404: // HttpStatusCode.NotFound
                    return AuthenticationException.Create(CommonErrorCodes.NotFound, exception.Message);
                case 408: // HttpStatusCode.RequestTimeout
                    return AuthenticationException.Create(CommonErrorCodes.Timeout, exception.Message);
                case 429: // HttpStatusCode.TooManyRequests
                    return AuthenticationException.Create(CommonErrorCodes.TooManyRequests, exception.Message);
                default:
                    return AuthenticationException.Create(CommonErrorCodes.InvalidRequest, exception.Message);
            }
        }

        static RequestFailedException CreateUnknownException(Exception exception)
        {
            return AuthenticationException.Create(CommonErrorCodes.Unknown, $"Unknown Error: {exception.Message}");
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
