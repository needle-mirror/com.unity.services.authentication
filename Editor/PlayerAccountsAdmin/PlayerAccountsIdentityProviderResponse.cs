using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.Editor
{
    /// <summary>
    /// A game's own social provider app (Apple, Google or Facebook) registered with Unity Player Accounts, so Player
    /// Accounts also accepts sign-in through it. Unity's shared app for that provider keeps working alongside it; the
    /// game chooses which one it signs in with.
    /// </summary>
    [Serializable]
    class PlayerAccountsIdentityProviderResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("projectId")]
        public string ProjectId { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("providerClientId")]
        public string ProviderClientId { get; set; }

        [JsonProperty("disabled")]
        public bool Disabled { get; set; }

        [JsonProperty("createdAt")]
        public string CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public string UpdatedAt { get; set; }

        [Preserve]
        public PlayerAccountsIdentityProviderResponse() {}
    }
}
