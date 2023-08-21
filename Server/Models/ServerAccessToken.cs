using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.Server
{
    class ServerAccessToken : BaseJwt
    {
        [Preserve]
        public ServerAccessToken() { }

        [JsonProperty("aud")]
        public string[] Audience;

        [JsonProperty("iss")]
        public string Issuer;

        [JsonProperty("jti")]
        public string JwtId;

        [JsonProperty("scopes")]
        public string[] Scope;

        [JsonProperty("sub")]
        public string Subject;
    }
}
