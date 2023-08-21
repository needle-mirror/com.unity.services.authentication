using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Services.Authentication
{
    class CodeLinkInfoRequest
    {
        [JsonProperty("signInCode")]
        public string SignInCode { get; set; }
    }
}
