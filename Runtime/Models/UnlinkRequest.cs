using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.Models
{
    /// <summary>
    /// Contains an unlink request information.
    /// </summary>
    [Serializable]
    public class UnlinkRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        [Preserve]
        public UnlinkRequest() {}

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
