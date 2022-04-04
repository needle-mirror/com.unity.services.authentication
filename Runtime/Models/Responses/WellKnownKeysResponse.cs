using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    class WellKnownKeysResponse
    {
        [Preserve]
        public WellKnownKeysResponse() {}

        [JsonProperty("keys")]
        public WellKnownKey[] Keys;
    }
}
