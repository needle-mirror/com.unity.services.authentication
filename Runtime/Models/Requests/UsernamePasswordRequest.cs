using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    [Serializable]
    class UsernamePasswordRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        [Preserve]
        internal UsernamePasswordRequest() {}

        /// <summary>
        /// The player's username
        /// </summary>
        [JsonProperty("username")]
        public string Username;

        /// <summary>
        /// The player's password.
        /// </summary>
        [JsonProperty("password")]
        public string Password;
    }
}
