using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    class LinkWithSteamRequest : LinkWithExternalTokenRequest
    {
        [Preserve]
        internal LinkWithSteamRequest() {}

        /// <summary>
        /// Option to add steam configuration
        /// </summary>
        [JsonProperty("steamConfig")]
        public SteamConfig SteamConfig;
    }
}
