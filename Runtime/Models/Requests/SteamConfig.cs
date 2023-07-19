using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    class SteamConfig
    {
        /// <summary>
        /// Constructor
        /// </summary>
        [Preserve]
        internal SteamConfig() {}

        /// <summary>
        /// Steam identity field to identify the calling service.
        /// </summary>
        [JsonProperty("identity")]
        public string identity;
    }
}
