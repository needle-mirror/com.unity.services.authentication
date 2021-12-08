using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.Models
{
    /// <summary>
    /// Contains User Information
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        [Preserve]
        public UserInfo() {}
        /// <summary>
        /// User Id
        /// </summary>
        [JsonProperty("id")]
        public string Id;

        /// <summary>
        /// User Id Domain
        /// </summary>
        [JsonProperty("idDomain")]
        public string IdDomain;

        /// <summary>
        /// User Creation date
        /// </summary>
        [JsonProperty("createdAt")]
        public string CreatedAt;

        /// <summary>
        /// User External Ids
        /// </summary>
        [JsonProperty("externalIds")]
        public List<ExternalId> ExternalIds;

        /// <summary>
        /// Gets User External Ids
        /// </summary>
        internal string GetExternalId(string providerId)
        {
            return ExternalIds?.FirstOrDefault(x => x.ProviderId == providerId)?.ExtId;
        }

        /// <summary>
        /// Add External Id to the User Info
        /// </summary>
        internal void AddExternalId(ExternalId externalId)
        {
            if (externalId == null)
                return;

            if (ExternalIds == null)
            {
                ExternalIds = new List<ExternalId>();
            }

            ExternalIds.Add(externalId);
        }

        /// <summary>
        /// Removes External Id
        /// </summary>
        internal void RemoveExternalId(string providerId)
        {
            ExternalIds?.RemoveAll(x => x.ProviderId == providerId);
        }
    }
}
