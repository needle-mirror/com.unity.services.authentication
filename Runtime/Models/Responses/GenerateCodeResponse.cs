using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    [Serializable]
    class GenerateCodeResponse
    {
        [Preserve]
        public GenerateCodeResponse() {}

        [JsonProperty("codeLinkSessionId")]
        public string CodeLinkSessionId;

        [JsonProperty("signInCode")]
        public string SignInCode;

        [JsonProperty("expiration")]
        public string Expiration;
    }
}
