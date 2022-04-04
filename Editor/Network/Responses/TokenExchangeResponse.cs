using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.Editor
{
    [Serializable]
    class TokenExchangeResponse
    {
        [JsonProperty("token")]
        public string Token;

        [Preserve]
        public TokenExchangeResponse() {}
    }
}
