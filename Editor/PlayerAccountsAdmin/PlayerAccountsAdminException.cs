using System;

namespace Unity.Services.Authentication.Editor
{
    /// <summary>
    /// A failed call to the Player Accounts or Authentication admin API, classified so callers can say what to fix.
    /// </summary>
    class PlayerAccountsAdminException : Exception
    {
        internal enum FailureReason
        {
            /// <summary>The Editor's Unity sign-in is missing or expired.</summary>
            Unauthorized,
            /// <summary>The signed-in account lacks the permission the call needs.</summary>
            Forbidden,
            /// <summary>What the call names (or the project) does not exist.</summary>
            NotFound,
            /// <summary>The server rejected the request's content.</summary>
            InvalidRequest,
            /// <summary>The server could not be reached.</summary>
            Network,
            Unknown,
        }

        internal FailureReason Reason { get; }
        internal long StatusCode { get; }

        /// <summary>The server's own explanation, when it sent one.</summary>
        internal string ServerDetail { get; }

        internal PlayerAccountsAdminException(FailureReason reason, long statusCode, string serverDetail, Exception innerException = null)
            : base(serverDetail ?? reason.ToString(), innerException)
        {
            Reason = reason;
            StatusCode = statusCode;
            ServerDetail = serverDetail;
        }
    }
}
