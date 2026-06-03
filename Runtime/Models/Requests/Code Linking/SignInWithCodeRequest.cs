using System;
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

        /// <summary>
        /// Attributes to be considered when determining player variant tags.
        /// </summary>
        [JsonProperty("attributes")]
        public Attributes Attributes;
    }
}
