using System;
using Newtonsoft.Json;

namespace Unity.Services.Authentication
{
    [Serializable]
    class ConfirmSignInCodeRequest
    {
        [JsonProperty("signInCode")]
        public string SignInCode { get; set; }

        [JsonProperty("idProvider")]
        public string IdProvider;

        [JsonProperty("externalToken")]
        public string ExternalToken;

        [JsonProperty("sessionToken")]
        public string SessionToken;
    }
}
