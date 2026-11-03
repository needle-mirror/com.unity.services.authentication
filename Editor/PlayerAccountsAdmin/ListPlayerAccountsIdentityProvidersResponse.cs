using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.Editor
{
    [Serializable]
    class ListPlayerAccountsIdentityProvidersResponse
    {
        [JsonProperty("results")]
        public PlayerAccountsIdentityProviderResponse[] Results { get; set; }

        [Preserve]
        public ListPlayerAccountsIdentityProvidersResponse() {}
    }
}
