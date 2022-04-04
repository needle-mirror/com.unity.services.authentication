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

        [Preserve]
        public UpdateIdProviderRequest() {}

        [Preserve]
        public UpdateIdProviderRequest(IdProvider idProvider)
        {
            ClientId = idProvider.ClientId;
            ClientSecret = idProvider.ClientSecret;
            Type = idProvider.Type;
        }
    }
}
