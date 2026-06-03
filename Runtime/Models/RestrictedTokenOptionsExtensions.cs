namespace Unity.Services.Authentication.Internal
{
    /// <summary>
    /// Extensions on <see cref="RestrictedTokenOptions"/> reserved for Unity-internal callers.
    /// These are not part of the supported public API; do not call from game code.
    /// </summary>
    public static class RestrictedTokenOptionsExtensions
    {
        /// <summary>
        /// Sets the ISO-3166-1 alpha-2 country code to include in the issued token.
        /// </summary>
        /// <param name="options">The <see cref="RestrictedTokenOptions"/> to configure.</param>
        /// <param name="country">The ISO-3166-1 alpha-2 country code to set.</param>
        public static void SetCountry(this RestrictedTokenOptions options, string country)
        {
            options.Country = country;
        }
    }
}
