using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.Editor
{
    [Serializable]
    class GetIdDomainResponse
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("name")]
        public string Name;

        [Preserve]
        public GetIdDomainResponse() {}
    }
}
