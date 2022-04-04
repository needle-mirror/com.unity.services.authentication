using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    [Serializable]
    class UnlinkResponse
    {
        [Preserve]
        public UnlinkResponse() {}

        [JsonProperty("user")]
        public User User;
    }
}
