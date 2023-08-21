using System;
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
        /// <param name="errorCode">Gets the error code for the current exception</param>
        /// <param name="message">Gets a message that describes the current exception.</param>
        /// <param name="innerException">Gets the Exception instance that caused the current exception.</param>
        /// <returns>The built ServerAuthenticationException</returns>
        internal static ServerAuthenticationException Create(int errorCode, string message, Exception innerException = null)
        {
            return new ServerAuthenticationException(errorCode, message, innerException);
        }
    }
}
