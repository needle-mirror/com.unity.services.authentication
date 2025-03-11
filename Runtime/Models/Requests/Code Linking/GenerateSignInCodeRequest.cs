using System;
using Newtonsoft.Json;

namespace Unity.Services.Authentication
{
    [Serializable]
    class GenerateSignInCodeRequest
    {
        [JsonProperty("identifier")]
        public string Identifier;

        [JsonProperty("codeChallenge")]
        public string CodeChallenge;
    }
}
