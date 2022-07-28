using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.Editor
{
    [Serializable]
    class CreateIdProviderRequest
    {
        [JsonProperty("clientId")]
        public string ClientId;

        [JsonProperty("clientSecret")]
        public string ClientSecret;

        [JsonProperty("oidcConfig")]
        public OpenIDConfig OidcConfig;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("disabled")]
        public bool Disabled;

        [Preserve]
        public CreateIdProviderRequest() {}

        [Preserve]
        public CreateIdProviderRequest(IdProvider idProvider)
        {
            ClientId = idProvider.ClientId;
            ClientSecret = idProvider.ClientSecret;
            Type = idProvider.Type;
            Disabled = idProvider.Disabled;
            OidcConfig = idProvider.OidcConfig;
        }
    }
}
