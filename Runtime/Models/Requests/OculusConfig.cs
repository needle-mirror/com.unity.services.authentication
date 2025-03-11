using Newtonsoft.Json;
using System;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// Configuration for an Oculus Id provider.
    /// </summary>
    [Serializable]
    class OculusConfig
    {
        /// <summary>
        /// Constructor
        /// </summary>
        [Preserve]
        internal OculusConfig() {}

        /// <summary>
        /// Oculus account userId
        /// </summary>
        [JsonProperty("userId")]
        public string UserId;
    }
}
