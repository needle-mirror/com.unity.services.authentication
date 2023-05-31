using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// Contains Player Information
    /// </summary>
    [Serializable]
    class User
    {
        /// <summary>
        /// Constructor
        /// </summary>
        [Preserve]
        public User() {}

        /// <summary>
        /// Player Id
        /// </summary>
        [JsonProperty("id")]
        public string Id;

        /// <summary>
        /// Player Creation date
        /// </summary>
        [JsonProperty("createdAt")]
        public string CreatedAt;

        /// <summary>
        /// Player External Ids
        /// </summary>
        [JsonProperty("externalIds")]
        public List<ExternalIdentity> ExternalIds;

        /// <summary>
        /// Username and Password information
        /// </summary>
        [JsonProperty("username")]
        [CanBeNull]
        public string Username;

        /// <summary>
        /// Username Information
        /// </summary>
        [JsonProperty("UsernameInfo")]
        [CanBeNull]
        public UsernameInfo UsernameInfo;
    }
}
