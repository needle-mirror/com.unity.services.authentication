using Newtonsoft.Json;
using System;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    [Serializable]
    class LinkWithAppleGameCenterRequest : LinkWithExternalTokenRequest
    {
        [Preserve]
        internal LinkWithAppleGameCenterRequest() {}

        /// <summary>
        /// Parameters to add an AppleGameCenter config
        /// </summary>
        [JsonProperty("appleGameCenterConfig")]
        public AppleGameCenterConfig AppleGameCenterConfig;
    }
}
