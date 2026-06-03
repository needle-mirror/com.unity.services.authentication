using Newtonsoft.Json;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// Represents a request to sign in anonymously.
    /// </summary>
    internal class SignInAnonymouslyRequest
    {
        /// <summary>
        /// Attributes to be considered when determining player variant tags.
        /// </summary>
        [JsonProperty("attributes")]
        public Attributes Attributes;
    }
}
