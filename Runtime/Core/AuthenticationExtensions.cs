using System.Text.RegularExpressions;
using Unity.Services.Core;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// Authentication extension methods
    /// </summary>
    public static class AuthenticationExtensions
    {
        internal const string ProfileKey = "com.unity.services.authentication.profile";
        const string k_ProfileRegex = @"^[a-zA-Z0-9_-]{1,30}$";

        /// <summary>
        /// An extension to set the profile name to use. The profile is a namespace in PlayerPrefs where the values from the Authentication SDK are saved.
        /// Setting a profile name in this way is optional, by default the value "default" is used.
        /// The profile name must be alphanumeric, '-', or '_', and no longer than 30 characters.
        /// </summary>
        /// <param name="options">The InitializationOptions object to modify</param>
        /// <param name="profile">The profile name to use</param>
        /// <exception cref="AuthenticationException">
        /// the method will fail with an AuthenticationException if an invalid profile name is passed
        /// <list type="bullet">
        /// <item><description>Throws with <c>ErrorCode</c> <see cref="AuthenticationErrorCodes.ClientInvalidProfile"/> if the profile name is invalid.</description></item>
        /// </list>
        /// </exception>
        /// <returns>
        /// Return <paramref name="options"/>.
        /// Fluent interface pattern to make it easier to chain set options operations.
        /// </returns>
        public static InitializationOptions SetProfile(this InitializationOptions options, string profile)
        {
            if (string.IsNullOrEmpty(profile) || !Regex.Match(profile, k_ProfileRegex).Success)
            {
                throw AuthenticationException.Create(AuthenticationErrorCodes.ClientInvalidProfile, "Invalid profile name. The profile may only contain alphanumeric values, '-', '_', and must be no longer than 30 characters.");
            }
            return options.SetOption(ProfileKey, profile);
        }
    }
}
