using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    [Serializable]
    class CodeLinkInfoResponse
    {
        [Preserve]
        public CodeLinkInfoResponse() {}

        [JsonProperty("identifier")]
        public string Identifier;

        [JsonProperty("expiration")]
        public string Expiration;
    }
}
