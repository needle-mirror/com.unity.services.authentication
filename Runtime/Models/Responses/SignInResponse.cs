using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    [Serializable]
    class SignInResponse
    {
        [Preserve]
        public SignInResponse() {}

        [JsonProperty("userId")]
        public string UserId;

        [JsonProperty("idToken")]
        public string IdToken;

        [JsonProperty("sessionToken")]
        public string SessionToken;

        [JsonProperty("expiresIn")]
        public int ExpiresIn;

        [JsonProperty("user")]
        public User User;
    }
}
