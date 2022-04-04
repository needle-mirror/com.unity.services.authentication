using Unity.Services.Core;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// Authentication extension methods
    /// </summary>
    public static class AuthenticationExtensions
    {
        internal const string ProfileKey = "com.unity.services.authentication.profile";

        /// <summary>
        /// An extension to set the profile to use.
        /// </summary>
        /// <param name="options">The InitializationOptions object to modify</param>
        /// <param name="profile">The profile to use</param>
        /// <returns>
        /// Return <paramref name="options"/>.
        /// Fluent interface pattern to make it easier to chain set options operations.
        /// </returns>
        public static InitializationOptions SetProfile(this InitializationOptions options, string profile)
        {
            return options.SetOption(ProfileKey, profile);
        }
    }
}
