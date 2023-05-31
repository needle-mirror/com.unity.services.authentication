using Newtonsoft.Json;
using System;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    [Serializable]
    class SignInWithOculusRequest : SignInWithExternalTokenRequest
    {
        [Preserve]
        internal SignInWithOculusRequest() {}

        /// <summary>
        /// Option to add oculus config
        /// </summary>
        [JsonProperty("oculusConfig")]
        public OculusConfig OculusConfig;
    }
}
