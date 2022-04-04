using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// Contains an unlink request information.
    /// </summary>
    [Serializable]
    class UnlinkRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        [Preserve]
        internal UnlinkRequest() {}

        /// <summary>
        /// The external provider type id.
        /// </summary>
        [JsonProperty("idProvider")]
        public string IdProvider;

        /// <summary>
        /// The external id
        /// </summary>
        [JsonProperty("externalId")]
        public string ExternalId;
    }
}
