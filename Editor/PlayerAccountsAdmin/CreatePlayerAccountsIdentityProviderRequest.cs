using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.Editor
{
    [Serializable]
    class CreatePlayerAccountsIdentityProviderRequest
    {
        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("providerClientId")]
        public string ProviderClientId { get; set; }

        [Preserve]
        public CreatePlayerAccountsIdentityProviderRequest() {}
    }
}
