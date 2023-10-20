using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.PlayerAccounts
{
    /// <summary>
    /// Represents the claims contained within an ID token obtained during sign-in.
    /// </summary>
    public class IdToken : BaseJwt
    {
        /// <summary>
        /// Constructor
        /// </summary>
        [Preserve]
        internal IdToken() { }

        /// <summary>
        /// Gets the audience of the ID token.
        /// </summary>
        [JsonProperty("aud")]
        public string[] Audience;

        /// <summary>
        /// Gets the email of the user.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Indicates whether the user's email address is verified.
        /// </summary>
        [JsonProperty("email_verified")]
        public bool EmailVerified { get; set; }

        /// <summary>
        /// Indicates whether the user's email address is private.
        /// </summary>
        [JsonProperty("is_private_email")]
        public bool IsPrivateEmail { get; set; }

        /// <summary>
        /// Gets the issuer of the ID token.
        /// </summary>
        [JsonProperty("iss")]
        public string Issuer;

        /// <summary>
        /// Gets the JWT ID of the ID token.
        /// </summary>
        [JsonProperty("jti")]
        public string JwtId;

        /// <summary>
        /// Gets the nonce value used during the authentication request.
        /// </summary>
        [JsonProperty("nonce")]
        public string Nonce { get; set; }

        /// <summary>
        /// Gets the subject identifier of the user.
        /// </summary>
        [JsonProperty("sub")]
        public string Subject;
    }
}
