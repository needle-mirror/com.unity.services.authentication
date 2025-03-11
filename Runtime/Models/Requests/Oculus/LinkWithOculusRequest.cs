using Newtonsoft.Json;
using System;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    [Serializable]
    class LinkWithOculusRequest : LinkWithExternalTokenRequest
    {
        [Preserve]
        internal LinkWithOculusRequest() {}

        /// <summary>
        /// Option to add oculus config
        /// </summary>
        [JsonProperty("oculusConfig")]
        public OculusConfig OculusConfig;
    }
}
