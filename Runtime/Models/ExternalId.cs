using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.Models
{
    /// <summary>
    /// Contains elements for ExternalId
    /// </summary>
    [Serializable]
    public class ExternalId
    {
        /// <summary>
        /// Constructor
        /// </summary>
        [Preserve]
        public ExternalId() {}

        /// <summary>
        /// The external provider type id.
        /// </summary>
        [JsonProperty("providerId")]
        public string ProviderId;

        /// <summary>
        /// The external id
        /// </summary>
        [JsonProperty("externalId")]
        public string ExtId;
    }
}
