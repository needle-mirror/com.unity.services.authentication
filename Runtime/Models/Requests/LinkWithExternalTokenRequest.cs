using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// Contains an external provider authentication information.
    /// </summary>
    [Serializable]
    class LinkWithExternalTokenRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        [Preserve]
        internal LinkWithExternalTokenRequest() {}

        /// <summary>
        /// The external provider type id.
        /// </summary>
        [JsonProperty("idProvider")]
        public string IdProvider;

        /// <summary>
        /// The external provider authentication token.
        /// </summary>
        [JsonProperty("token")]
        public string Token;

        /// <summary>
        /// Option to force the link in case the account is already linked to another player.
        /// </summary>
        [JsonProperty("forceLink")]
        public bool ForceLink;
    }
}
