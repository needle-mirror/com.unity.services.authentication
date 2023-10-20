using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication.PlayerAccounts
{
    /// <summary>
    /// Represents a json web token
    /// </summary>
    public class BaseJwt
    {
        /// <summary>
        /// Constructor
        /// </summary>
        [Preserve]
        internal BaseJwt() {}

        /// <summary>
        /// Expiration Time Claim based on the unix time format.
        /// </summary>
        [JsonProperty("exp")]
        public int ExpirationTimeUnix;

        /// <summary>
        /// Issued At Time Claim based on the unix time format.
        /// </summary>
        [JsonProperty("iat")]
        public int IssuedAtTimeUnix;

        /// <summary>
        /// Not Before Claim based on the unix time format.
        /// </summary>
        [JsonProperty("nbf")]
        public int NotBeforeTimeUnix;

        /// <summary>
        /// The converted expiration time
        /// </summary>
        [JsonIgnore]
        public DateTime ExpirationTime => ConvertTimestamp(ExpirationTimeUnix);

        /// <summary>
        /// The converted issued at time
        /// </summary>
        [JsonIgnore]
        public DateTime IssuedAtTime => ConvertTimestamp(IssuedAtTimeUnix);

        /// <summary>
        /// The converted not before time
        /// </summary>
        [JsonIgnore]
        public DateTime NotBeforeTime => ConvertTimestamp(NotBeforeTimeUnix);

        internal DateTime ConvertTimestamp(int timestamp)
        {
            if (timestamp != 0)
            {
                var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                return dateTimeOffset.DateTime;
            }

            throw new Exception("Token does not contain a value for this timestamp.");
        }
    }
}
