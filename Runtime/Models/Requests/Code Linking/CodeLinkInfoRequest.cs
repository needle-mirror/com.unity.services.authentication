using System;
using Newtonsoft.Json;

namespace Unity.Services.Authentication
{
    [Serializable]
    class CodeLinkInfoRequest
    {
        [JsonProperty("signInCode")]
        public string SignInCode;
    }
}
