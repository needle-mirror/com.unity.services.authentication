using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Services.Authentication
{
    class SignInWithOculusRequest : SignInWithExternalTokenRequest
    {
        /// <summary>
        /// Option to add oculus config
        /// </summary>
        [JsonProperty("oculusConfig")]
        public OculusConfig OculusConfig;
    }
}
