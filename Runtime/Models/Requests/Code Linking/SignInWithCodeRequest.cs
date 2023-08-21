using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Unity.Services.Authentication
{
    [Serializable]
    class SignInWithCodeRequest
    {
        [JsonProperty("codeLinkSessionId")]
        public string CodeLinkSessionId;

        [JsonProperty("codeVerifier")]
        public string CodeVerifier;
    }
}
