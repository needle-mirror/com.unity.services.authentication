using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Services.Authentication.Editor
{
    /// <summary>
    /// OpenID connect Id provider configuration.
    /// </summary>
    struct OpenIDConfig
    {
        /// <summary>
        /// Issuer URL that uniquely identifies an OpenID Connect Id provider.
        /// </summary>
        [JsonProperty("issuer")]
        public string Issuer { get; set; }
    }
}
