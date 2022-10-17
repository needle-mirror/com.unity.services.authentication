using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// Configuration for an Oculus Id provider.
    /// </summary>
    [Serializable]
    struct OculusConfig
    {
        /// <summary>
        /// Oculus account userId
        /// </summary>
        [JsonProperty("userId")]
        public string UserId { get; set; }
    }
}
