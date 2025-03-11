using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// Contains elements for ExternalId
    /// </summary>
    [Serializable]
    class ExternalIdentity
    {
        /// <summary>
        /// Constructor
        /// </summary>
        [Preserve]
        public ExternalIdentity() {}

        /// <summary>
        /// The external provider type id.
        /// </summary>
        [JsonProperty("providerId")]
        public string ProviderId;

        /// <summary>
        /// The external id
        /// </summary>
        [JsonProperty("externalId")]
        public string ExternalId;
    }
}
