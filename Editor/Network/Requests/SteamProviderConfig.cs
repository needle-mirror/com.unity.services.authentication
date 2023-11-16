using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Services.Authentication.Editor
{
    /// <summary>
    /// SteamProviderConfig configuration.
    /// </summary>
    class SteamProviderConfig
    {
        /// <summary>
        /// Array of Steam appId configurations, each holding a description and appId.
        /// </summary>
        [JsonProperty("additionalAppIds")]
        public List<AdditionalAppId> AdditionalAppIds { get; set; }
    }

    class AdditionalAppId
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("appId")]
        public string AppId { get; set; }

        public bool New { get; set; }

        public static AdditionalAppId EmptyAdditionalAppId()
        {
            return new AdditionalAppId { New = true};
        }

        public AdditionalAppId Clone()
        {
            return new AdditionalAppId
            {
                Description = Description,
                AppId = AppId,
                New = true
            };
        }
    }
}
