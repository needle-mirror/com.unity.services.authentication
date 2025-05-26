using System;
using Unity.Services.Authentication.Server.Shared;
using Unity.Services.Core;

namespace Unity.Services.Authentication.Server
{
    /// <summary>
    /// ServerAuthenticationException represents a runtime exception from server authentication.
    /// </summary>
    /// <remarks>
    /// See <see cref="ServerAuthenticationErrorCodes"/> and <see cref="CommonErrorCodes"/>for possible error codes.
    /// Consult the service documentation for specific error codes various APIs can return.
    /// </remarks>
    public sealed class ServerAuthenticationException : RequestFailedException
    {
        /// <summary>
        /// Constructor of the ServerAuthenticationException with the error code, a message, and inner exception.
        /// </summary>
        /// <param name="errorCode">The error code for ServerAuthenticationException.</param>
        /// <param name="message">The additional message that helps to debug the error.</param>
        /// <param name="innerException">The inner exception reference.</param>
        ServerAuthenticationException(int errorCode, string message, Exception innerException = null)
            : base(errorCode, message, innerException)
        {
        }

        /// <summary>
        /// Creates the exception base on errorCode range.
        /// </summary>
        /// <param name="errorCode">The error code for the current exception</param>
        /// <param name="message">The message that describes the current exception.</param>
        /// <param name="innerException">The exception instance that caused the current exception.</param>
        /// <returns>The built ServerAuthenticationException</returns>
        internal static ServerAuthenticationException Create(int errorCode, string message, Exception innerException = null)
        {
            return new ServerAuthenticationException(errorCode, message, innerException);
        }

        /// <summary>
        /// Creates the exception base on errorCode range.
        /// </summary>
        /// <param name="errorCode">The error code for the current exception</param>
        /// <param name="exception">The api exception from the request</param>
        /// <returns>The built ServerAuthenticationException</returns>
        internal static ServerAuthenticationException Create(int errorCode, ApiException exception)
        {
            return new ServerAuthenticationException(errorCode, $"{exception.Message}\n{exception.Response?.RawContent}");
        }
    }
}
