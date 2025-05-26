using System;
using Unity.Services.Authentication.Server.Shared;
using Unity.Services.Core;

namespace Unity.Services.Authentication.Server
{
    static class ServerAuthenticationExceptionFactory
    {
        public static ServerAuthenticationException Create(ApiException exception)
        {
            switch (exception?.Type)
            {
                case ApiExceptionType.InvalidParameters:
                    return ServerAuthenticationException.Create(ServerAuthenticationErrorCodes.InvalidParameters, exception);
                case ApiExceptionType.Deserialization:
                    return ServerAuthenticationException.Create(CommonErrorCodes.Unknown, exception);
                case ApiExceptionType.Network: // 5XX
                    return CreateNetworkException(exception);
                case ApiExceptionType.Http: // 4XX
                    return CreateHttpException(exception);
                default:
                    return CreateUnknownException(exception);
            }
        }

        public static ServerAuthenticationException Create(Exception exception)
        {
            return CreateUnknownException(exception);
        }

        public static ServerAuthenticationException CreateClientInvalidState(ServerAuthenticationState currentState)
        {
            var errorMessage = string.Empty;

            switch (currentState)
            {
                case ServerAuthenticationState.Unauthorized:
                    errorMessage = "Invalid state for this operation. Currently signed out.";
                    break;
                case ServerAuthenticationState.SigningIn:
                    errorMessage = "Invalid state for this operation. Already signing in.";
                    break;
                case ServerAuthenticationState.Authorized:
                case ServerAuthenticationState.Refreshing:
                    errorMessage = "Invalid state for this operation. Already signed in.";
                    break;
                case ServerAuthenticationState.Expired:
                    errorMessage = "Invalid state for this operation. Session has expired.";
                    break;
            }

            return ServerAuthenticationException.Create(ServerAuthenticationErrorCodes.ClientInvalidUserState, errorMessage);
        }

        static ServerAuthenticationException CreateNetworkException(ApiException exception)
        {
            switch (exception?.Response?.StatusCode)
            {
                case 503: // HttpStatusCode.ServiceUnavailable
                    return ServerAuthenticationException.Create(CommonErrorCodes.ServiceUnavailable, exception);
                case 504: // HttpStatusCode.GatewayTimeout
                    return ServerAuthenticationException.Create(CommonErrorCodes.Timeout, exception);
                default:
                    return ServerAuthenticationException.Create(CommonErrorCodes.TransportError, exception);
            }
        }

        static ServerAuthenticationException CreateHttpException(ApiException exception)
        {
            switch (exception?.Response?.StatusCode)
            {
                case 400: // HttpStatusCode.BadRequest
                    return ServerAuthenticationException.Create(CommonErrorCodes.InvalidRequest, exception);
                case 401: // HttpStatusCode.Unauthorized
                    return ServerAuthenticationException.Create(CommonErrorCodes.InvalidToken, exception);
                case 403: // HttpStatusCode.Forbidden
                    return ServerAuthenticationException.Create(CommonErrorCodes.Forbidden, exception);
                case 404: // HttpStatusCode.NotFound
                    return ServerAuthenticationException.Create(CommonErrorCodes.NotFound, exception);
                case 408: // HttpStatusCode.RequestTimeout
                    return ServerAuthenticationException.Create(CommonErrorCodes.Timeout, exception);
                case 429: // HttpStatusCode.TooManyRequests
                    return ServerAuthenticationException.Create(CommonErrorCodes.TooManyRequests, exception);
                default:
                    return ServerAuthenticationException.Create(CommonErrorCodes.InvalidRequest, exception);
            }
        }

        static ServerAuthenticationException CreateUnknownException(Exception exception)
        {
            return ServerAuthenticationException.Create(CommonErrorCodes.Unknown, $"Unknown Error: {exception.Message}");
        }
    }
}
