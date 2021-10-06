using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.Models
{
    /// <summary>
    /// Contains an external provider authentication information.
    /// </summary>
    [Serializable]
    public class ExternalTokenRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        [Preserve]
        public ExternalTokenRequest() {}

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
    }
}
