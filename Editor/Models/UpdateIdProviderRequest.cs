using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.Editor.Models
{
    [Serializable]
    class UpdateIdProviderRequest
    {
        [Preserve]
        public UpdateIdProviderRequest() {}

        [Preserve]
        public UpdateIdProviderRequest(IdProviderResponse body)
        {
            ClientId = body.ClientId;
            ClientSecret = body.ClientSecret;
            Type = body.Type;
        }

        [JsonProperty("clientId")]
        public string ClientId;

        [JsonProperty("clientSecret")]
        public string ClientSecret;

        [JsonProperty("type")]
        public string Type;

        public override bool Equals(Object obj)
        {
            // Check for null and compare run-time types.
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            UpdateIdProviderRequest c = (UpdateIdProviderRequest)obj;
            return (ClientId == c.ClientId) &&
                (ClientSecret == c.ClientSecret) &&
                (Type == c.Type);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ClientId != null ? ClientId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ClientSecret != null ? ClientSecret.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
