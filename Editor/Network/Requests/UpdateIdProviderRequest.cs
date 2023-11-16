using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.Editor
{
    [Serializable]
    class UpdateIdProviderRequest
    {
        [JsonProperty("clientId")]
        public string ClientId;

        [JsonProperty("clientSecret")]
        public string ClientSecret;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("oidcConfig")]
        public OpenIDConfig OidcConfig;

        [JsonProperty("relyingParty")]
        public string RelyingParty;

        [JsonProperty("steamConfig")]
        public SteamProviderConfig SteamProviderConfig;

        [Preserve]
        public UpdateIdProviderRequest() {}

        [Preserve]
        public UpdateIdProviderRequest(IdProvider idProvider)
        {
            ClientId = idProvider.ClientId;
            ClientSecret = idProvider.ClientSecret;
            Type = idProvider.Type;
            OidcConfig = idProvider.OidcConfig;
            RelyingParty = idProvider.RelyingParty;
            SteamProviderConfig = idProvider.SteamProviderConfig;
        }
    }
}
