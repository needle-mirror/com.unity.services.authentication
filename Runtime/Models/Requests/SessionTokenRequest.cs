using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    [Serializable]
    class SessionTokenRequest
    {
        [Preserve]
        public SessionTokenRequest() {}

        [JsonProperty("sessionToken")]
        public string SessionToken;

        /// <summary>
        /// Attributes to be considered when determining player variant tags.
        /// </summary>
        [JsonProperty("attributes")]
        public Attributes Attributes;

        /// <summary>
        /// If true, the Release and Variant claims from the most recent authetication action associated with the session
        /// will be re-applied to the new token, instead of re-evaluating what claims should be applied.
        /// This helps to ensure that e.g. automated token refreshes do not change in-game behavior for players.
        /// </summary>
        [JsonProperty("retainTargetingClaims")]
        public bool RetainTargetingClaims;

        /// <summary>
        /// When true, the issued session token may only be used once.
        /// </summary>
        [JsonProperty("singleUse", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SingleUse;

        /// <summary>
        /// Service short names to restrict the issued ID token's audience to.
        /// </summary>
        [JsonProperty("audiences", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Services;

        /// <summary>
        /// Requested lifetime of the issued session token in seconds.
        /// </summary>
        [JsonProperty("ttlSeconds", NullValueHandling = NullValueHandling.Ignore)]
        public long? TtlSeconds;

        /// <summary>
        /// ISO-3166-1 alpha-2 country code included in the issued token.
        /// </summary>
        [JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
        public string Country;
    }
}
