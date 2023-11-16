using JetBrains.Annotations;
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

        /// <summary>
        /// App Id that was used to generate the ticket. Only required for additional app ids (e.g.: PlayTest, Demo, etc)
        /// </summary>
        [JsonProperty("appId")]
        [CanBeNull]
        public string appId;
    }
}
