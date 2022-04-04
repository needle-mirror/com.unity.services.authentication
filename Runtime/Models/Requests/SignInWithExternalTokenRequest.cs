using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// Contains an external provider authentication information.
    /// </summary>
    [Serializable]
    class SignInWithExternalTokenRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        [Preserve]
        internal SignInWithExternalTokenRequest() {}

        /// <summary>
        /// The external provider type id.
        /// To be removed when we drop legacy support.
        /// </summary>
        [JsonProperty("idProvider")]
        public string IdProvider;

        /// <summary>
        /// The external provider authentication token.
        /// </summary>
        [JsonProperty("token")]
        public string Token;

        /// <summary>
        /// Option to sign in only if an account exists.
        /// </summary>
        [JsonProperty("signInOnly")]
        public bool SignInOnly;
    }
}
