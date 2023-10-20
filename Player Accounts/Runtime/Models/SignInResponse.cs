using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.PlayerAccounts
{
    [Serializable]
    class SignInResponse
    {
        [Preserve]
        public SignInResponse() {}

        [JsonProperty("passport")]
        public string PassportId;

        [JsonProperty("userId")]
        public string UserId;

        [JsonProperty("access_token")]
        public string AccessToken;

        [JsonProperty("id_token")]
        public string IdToken;

        [JsonProperty("token_type")]
        public string tokenType;

        [JsonProperty("expires_in")]
        public int ExpiresIn;

        [JsonProperty("refresh_token")]
        public string RefreshToken;
    }
}
