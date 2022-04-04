using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.Editor
{
    [Serializable]
    class ListIdProviderResponse
    {
        [JsonProperty("total")]
        public int Total;

        [JsonProperty("results")]
        public IdProviderResponse[] Results;

        [Preserve]
        public ListIdProviderResponse() {}
    }
}
