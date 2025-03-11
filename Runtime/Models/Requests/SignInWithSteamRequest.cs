using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    [Serializable]
    class SignInWithSteamRequest : SignInWithExternalTokenRequest
    {
        [Preserve]
        internal SignInWithSteamRequest() {}

        /// <summary>
        /// Option to add Steam configuration
        /// </summary>
        [JsonProperty("steamConfig")]
        public SteamConfig SteamConfig;
    }
}
