using System.Collections.Generic;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// Options for <see cref="IAuthenticationService.GenerateRestrictedTokenAsync"/>.
    /// </summary>
    public class RestrictedTokenOptions
    {
        /// <summary>
        /// Restricts which services can accept the issued token. Refer to the documentation
        /// of the service the token is intended for to determine the value to use.
        /// </summary>
        public List<string> Services { get; set; }

        /// <summary>
        /// Requested lifetime of the issued token in seconds. Leave <c>null</c> to use the
        /// server default.
        /// </summary>
        public long? TtlSeconds { get; set; }

        /// <summary>
        /// When <c>true</c>, the issued token can only be used once.
        /// </summary>
        public bool SingleUse { get; set; }

        // Set by trusted Unity-internal callers via
        // Unity.Services.Authentication.Internal.RestrictedTokenOptionsExtensions.SetCountry.
        internal string Country { get; set; }
    }
}
