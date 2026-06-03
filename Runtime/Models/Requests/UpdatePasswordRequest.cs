using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    [Serializable]
    class UpdatePasswordRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        [Preserve]
        internal UpdatePasswordRequest() {}

        /// <summary>
        /// The current password to be validated and changed
        /// </summary>
        [JsonProperty("password")]
        public string Password;

        /// <summary>
        /// The new password to be updated
        /// </summary>
        [JsonProperty("newPassword")]
        public string NewPassword;

        /// <summary>
        /// Attributes to be considered when determining player variant tags.
        /// </summary>
        [JsonProperty("attributes")]
        public Attributes Attributes;
    }
}
