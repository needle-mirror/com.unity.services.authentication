using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    class UsernameInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        [Preserve]
        public UsernameInfo() {}

        /// <summary>
        /// Username of player
        /// </summary>
        [JsonProperty("username")]
        public string Username;

        /// <summary>
        /// Time the username was created
        /// </summary>
        [JsonProperty("createdAt")]
        public string CreatedAt;

        /// <summary>
        /// Time the user was last authenticated at with this sign in method
        /// </summary>
        [JsonProperty("lastLoginAt")]
        public string LastLoginAt;

        /// <summary>
        /// Time the password was last updated at
        /// </summary>
        [JsonProperty("passwordUpdatedAt")]
        public string PasswordUpdatedAt;
    }
}
