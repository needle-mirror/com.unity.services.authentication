using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// Configuration for an AppleGameCenter Id provider.
    /// </summary>
    [Serializable]
    class AppleGameCenterConfig
    {
        [Preserve]
        internal AppleGameCenterConfig() {}

        /// <summary>
        /// AppleGameCenter teamPlayerId
        /// </summary>
        [JsonProperty("teamPlayerId")]
        public string TeamPlayerId;

        /// <summary>
        /// AppleGameCenter publicKeyURL
        /// </summary>
        [JsonProperty("publicKeyUrl")]
        public string PublicKeyURL;

        /// <summary>
        /// AppleGameCenter salt
        /// </summary>
        [JsonProperty("salt")]
        public string Salt;

        /// <summary>
        /// AppleGameCenter timestamp
        /// </summary>
        [JsonProperty("timestamp")]
        public ulong Timestamp;
    }
}
