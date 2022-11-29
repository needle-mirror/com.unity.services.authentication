using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    [Serializable]
    class SignInWithAppleGameCenterRequest : SignInWithExternalTokenRequest
    {
        [Preserve]
        internal SignInWithAppleGameCenterRequest() {}

        /// <summary>
        /// Parameters to add an AppleGameCenter config
        /// </summary>
        [JsonProperty("appleGameCenterConfig")]
        public AppleGameCenterConfig AppleGameCenterConfig;
    }
}
