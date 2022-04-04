using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    [Serializable]
    class SessionTokenRequest
    {
        [Preserve]
        public SessionTokenRequest() {}

        [JsonProperty("sessionToken")]
        public string SessionToken;
    }
}
