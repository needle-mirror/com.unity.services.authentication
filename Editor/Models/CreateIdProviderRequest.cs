using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.Editor.Models
{
    [Serializable]
    class CreateIdProviderRequest
    {
        [Preserve]
        public CreateIdProviderRequest() {}

        [Preserve]
        public CreateIdProviderRequest(IdProviderResponse body)
        {
            ClientId = body.ClientId;
            ClientSecret = body.ClientSecret;
            Type = body.Type;
            Disabled = body.Disabled;
        }

        [JsonProperty("clientId")]
        public string ClientId;

        [JsonProperty("clientSecret")]
        public string ClientSecret;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("disabled")]
        public bool Disabled;

        public override bool Equals(Object obj)
        {
            // Check for null and compare run-time types.
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            CreateIdProviderRequest c = (CreateIdProviderRequest)obj;
            return (ClientId == c.ClientId) &&
                (ClientSecret == c.ClientSecret) &&
                (Disabled == c.Disabled) &&
                (Type == c.Type);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ClientId != null ? ClientId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ClientSecret != null ? ClientSecret.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Disabled.GetHashCode();
                return hashCode;
            }
        }
    }
}
