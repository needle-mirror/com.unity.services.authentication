using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// The model for error response from authentication server.
    /// </summary>
    /// <remarks>
    /// There is another field "details" in the error response. It provides additional details
    /// to the error. It's ignored in this deserialized class since it's not needed by the client SDK.
    /// </remarks>
    [Serializable]
    class AuthenticationErrorResponse
    {
        [Preserve]
        public AuthenticationErrorResponse() {}

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("detail")]
        public string Detail;

        [JsonProperty("status")]
        public int Status;

        [JsonProperty("details")]
#pragma warning disable UAC1001
        public List<object> Details;
#pragma warning restore UAC1001
    }
}
