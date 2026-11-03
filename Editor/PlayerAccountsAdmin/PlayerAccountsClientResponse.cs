using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.Editor
{
    /// <summary>
    /// A project's Unity Player Accounts client, as the Player Accounts admin API returns it. A project has one.
    /// </summary>
    [Serializable]
    class PlayerAccountsClientResponse
    {
        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("projectId")]
        public string ProjectId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("platforms")]
        public string[] Platforms { get; set; }

        [JsonProperty("termsOfServiceLink")]
        public string TermsOfServiceLink { get; set; }

        [JsonProperty("privacyPolicyLink")]
        public string PrivacyPolicyLink { get; set; }

        [JsonProperty("createdAt")]
        public string CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public string UpdatedAt { get; set; }

        [Preserve]
        public PlayerAccountsClientResponse() {}
    }
}
