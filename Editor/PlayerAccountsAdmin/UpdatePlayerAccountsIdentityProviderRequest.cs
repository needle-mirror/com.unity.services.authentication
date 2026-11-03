using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.Editor
{
    /// <summary>A change to a registered social provider app. A null field keeps its value.</summary>
    [Serializable]
    class UpdatePlayerAccountsIdentityProviderRequest
    {
        [JsonProperty("providerClientId", NullValueHandling = NullValueHandling.Ignore)]
        public string ProviderClientId { get; set; }

        [JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Disabled { get; set; }

        [Preserve]
        public UpdatePlayerAccountsIdentityProviderRequest() {}
    }
}
