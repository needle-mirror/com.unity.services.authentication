using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.Editor
{
    /// <summary>
    /// The settings of a project's Unity Player Accounts client, as the create (POST) and update (PUT) calls take
    /// them. On an update, a null field keeps its value and an empty link removes it.
    /// </summary>
    [Serializable]
    class PlayerAccountsClientRequest
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("platforms", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Platforms { get; set; }

        [JsonProperty("termsOfServiceLink", NullValueHandling = NullValueHandling.Ignore)]
        public string TermsOfServiceLink { get; set; }

        [JsonProperty("privacyPolicyLink", NullValueHandling = NullValueHandling.Ignore)]
        public string PrivacyPolicyLink { get; set; }

        [Preserve]
        public PlayerAccountsClientRequest() {}
    }
}
