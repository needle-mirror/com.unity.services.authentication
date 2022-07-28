using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Services.Authentication.Editor
{
    public struct OpenIDConfig
    {
        [JsonProperty("issuer")]
        public string Issuer { get; set; }
    }
}
