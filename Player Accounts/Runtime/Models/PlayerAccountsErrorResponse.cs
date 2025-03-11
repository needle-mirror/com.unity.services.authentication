using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.PlayerAccounts
{
    [Serializable]
    class PlayerAccountsErrorResponse
    {
        [Preserve]
        public PlayerAccountsErrorResponse() {}

        [JsonProperty("error")]
        public string Error;

        [JsonProperty("error_description")]
        public string Description;
    }
}
