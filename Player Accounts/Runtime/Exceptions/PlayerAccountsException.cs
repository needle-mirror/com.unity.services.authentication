using System;
using Unity.Services.Core;

namespace Unity.Services.Authentication.PlayerAccounts
{
    /// <summary>
    /// Represents an exception related to the Player Accounts service.
    /// This exception is thrown when an error occurs while processing a request.
    /// </summary>
    public sealed class PlayerAccountsException : RequestFailedException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerAccountsException"/> class.
        /// </summary>
        /// <param name="errorCode">The error code associated with the exception.</param>
        /// <param name="message">A message that describes the error.</param>
        /// <param name="innerException">The exception that caused the current exception, or null if none.</param>
        PlayerAccountsException(int errorCode, string message, Exception innerException = null)
            : base(errorCode, message, innerException) { }

        /// <summary>
        /// Creates a new instance of the <see cref="PlayerAccountsException"/> class.
        /// </summary>
        /// <param name="errorCode">The error code associated with the exception.</param>
        /// <param name="message">A message that describes the error.</param>
        /// <param name="innerException">The exception that caused the current exception, or null if none.</param>
        /// <returns>A new instance of the <see cref="PlayerAccountsException"/> class.</returns>
        internal static PlayerAccountsException Create(int errorCode, string message, Exception innerException = null)
        {
            return new PlayerAccountsException(errorCode, message, innerException);
        }
    }
}
