using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    [Serializable]
    class LinkResponse
    {
        [Preserve]
        public LinkResponse() {}

        [JsonProperty("user")]
        public User User;
    }
}
