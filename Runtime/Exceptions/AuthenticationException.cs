using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Unity.Services.Core;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// AuthenticationException represents a runtime exception from authentication.
    /// </summary>
    /// <remarks>
    /// See <see cref="AuthenticationErrorCodes"/> for possible error codes.
    /// Consult the service documentation for specific error codes various APIs can return.
    /// </remarks>
    public sealed class AuthenticationException : RequestFailedException
    {
        /// <summary>
        /// Caches the player's notifications if any are available or null if none are available.
        /// </summary>
        public List<Notification> Notifications { get; }

        /// <summary>
        /// Constructor of the AuthenticationException with the error code, a message, and inner exception.
        /// </summary>
        /// <param name="errorCode">The error code for AuthenticationException.</param>
        /// <param name="message">The additional message that helps to debug the error.</param>
        /// <param name="innerException">The inner exception reference.</param>
        /// <param name="notifications">List of notifications available to the player or null if none</param>
        AuthenticationException(int errorCode, string message, Exception innerException = null, List<Notification> notifications = null)
            : base(errorCode, message, innerException)
        {
            Notifications = notifications;
        }

        /// <summary>
        /// Creates the exception base on errorCode range.
        /// If the errorCode is less than AuthenticationErrorCodes.MinValue it creates a <see cref="RequestFailedException"/>.
        /// Otherwise it creates an <see cref="AuthenticationException"/>
        /// </summary>
        /// <param name="errorCode">Gets the error code for the current exception</param>
        /// <param name="message">Gets a message that describes the current exception.</param>
        /// <param name="innerException">Gets the Exception instance that caused the current exception.</param>
        /// <returns>The built exception, either an AuthenticationException or a RequestFailedException</returns>
        public static RequestFailedException Create(int errorCode, string message, Exception innerException = null)
        {
            return Create(errorCode, message, null, innerException);
        }

        internal static RequestFailedException Create(int errorCode, string message, List<Notification> notifications, Exception innerException = null)
        {
            if (errorCode < AuthenticationErrorCodes.MinValue)
            {
                return new RequestFailedException(errorCode, message, innerException);
            }
            else
            {
                return new AuthenticationException(errorCode, message, innerException, notifications);
            }
        }
    }
}
