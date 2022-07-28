using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.Editor
{
    [Serializable]
    class IdProviderResponse
    {
        [JsonProperty("clientId")]
        public string ClientId;

        [JsonProperty("clientSecret")]
        public string ClientSecret;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("oidcConfig")]
        public OpenIDConfig OidcConfig;

        [JsonProperty("disabled")]
        public bool Disabled;

        [Preserve]
        public IdProviderResponse() {}

        [Preserve]
        public IdProviderResponse(IdProvider idProvider)
        {
            ClientId = idProvider.ClientId;
            ClientSecret = idProvider.ClientSecret;
            Type = idProvider.Type;
            OidcConfig = idProvider.OidcConfig;
            Disabled = idProvider.Disabled;
        }

        public IdProviderResponse Clone()
        {
            return new IdProviderResponse
            {
                Type = Type,
                ClientId = ClientId,
                ClientSecret = ClientSecret,
                OidcConfig = OidcConfig,
                Disabled = Disabled
            };
        }
    }
}
