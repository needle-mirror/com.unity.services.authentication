using System.Collections.Generic;
using System.Linq;

namespace Unity.Services.Authentication.Editor
{
    /// <summary>
    /// The registry for ID providers.
    /// </summary>
    public static class IdProviderRegistry
    {
        /// <summary>
        /// Expose the internal ID provider options for testing purpose.
        /// </summary>
        internal static Dictionary<string, IdProviderOptions> IdProviderOptions { get; set; } = DefaultIdProviderOptions;

        /// <summary>
        /// All ID provider types, sorted by alphabetical order.
        /// </summary>
        internal static IEnumerable<string> AllNames => IdProviderOptions.Select(x => x.Value.DisplayName).OrderBy(s => s);

        /// <summary>
        /// The default set of ID provider options.
        /// </summary>
        static Dictionary<string, IdProviderOptions> DefaultIdProviderOptions => new Dictionary<string, IdProviderOptions>
        {
            [IdProviderKeys.Apple] = new IdProviderOptions
            {
                IdProviderType = IdProviderKeys.Apple,
                DisplayName = IdProviderNames.SignInWithApple,
                ClientIdDisplayName = "App ID",
                NeedClientSecret = false
            },
            [IdProviderKeys.Facebook] = new IdProviderOptions
            {
                IdProviderType = IdProviderKeys.Facebook,
                DisplayName = IdProviderNames.Facebook,
                ClientIdDisplayName = "App ID",
                ClientSecretDisplayName = "App Secret",
                NeedClientSecret = true
            },
            [IdProviderKeys.Google] = new IdProviderOptions
            {
                IdProviderType = IdProviderKeys.Google,
                DisplayName = IdProviderNames.Google,
                ClientIdDisplayName = "Client ID",
                NeedClientSecret = false
            },
            [IdProviderKeys.GooglePlayGames] = new IdProviderOptions
            {
                IdProviderType = IdProviderKeys.GooglePlayGames,
                DisplayName = IdProviderNames.GooglePlayGames,
                ClientIdDisplayName = "Client ID",
                ClientSecretDisplayName = "Client Secret",
                NeedClientSecret = true
            },
            [IdProviderKeys.OpenIDConnect] = new IdProviderOptions
            {
                IdProviderType = IdProviderKeys.OpenIDConnect,
                DisplayName = IdProviderNames.OpenIDConnect,
                ClientIdDisplayName = "Client ID",
                ClientSecretDisplayName = "Client Secret",
                NeedClientSecret = false,
                OidcConfig = new OpenIDConfig() { Issuer = "" }
            },
            [IdProviderKeys.Steam] = new IdProviderOptions
            {
                IdProviderType = IdProviderKeys.Steam,
                DisplayName = IdProviderNames.Steam,
                ClientIdDisplayName = "App ID",
                ClientSecretDisplayName = "Key",
                NeedClientSecret = true
            }
        };

        /// <summary>
        /// Reset the registry to use defaults.
        /// </summary>
        internal static void Reset()
        {
            IdProviderOptions = DefaultIdProviderOptions;
        }

        /// <summary>
        /// Register a new ID provider option.
        /// </summary>
        /// <param name="idProviderOptions">The new ID provider option to register.</param>
        public static void Register(IdProviderOptions idProviderOptions)
        {
            IdProviderOptions[idProviderOptions.IdProviderType] = idProviderOptions;
        }

        /// <summary>
        /// Unregister an ID provider option.
        /// </summary>
        /// <param name="idProviderType">The ID provider option to unregister.</param>
        public static void Unregister(string idProviderType)
        {
            IdProviderOptions.Remove(idProviderType);
        }

        /// <summary>
        /// Get an ID provider option by type.
        /// </summary>
        /// <param name="idProviderType">The ID provider type.</param>
        /// <returns>The ID provider option.</returns>
        public static IdProviderOptions GetOptions(string idProviderType)
        {
            if (idProviderType.Contains(IdProviderKeys.OpenIDConnect))
            {
                idProviderType = IdProviderKeys.OpenIDConnect;
            }

            if (!IdProviderOptions.ContainsKey(idProviderType))
            {
                return null;
            }
            return IdProviderOptions[idProviderType];
        }

        /// <summary>
        /// Get an ID provider option by type.
        /// </summary>
        /// <param name="idProviderName">The ID provider name.</param>
        /// <returns>The ID provider option.</returns>
        internal static IdProviderOptions GetOptionsByName(string idProviderName)
        {
            return IdProviderOptions.Values.FirstOrDefault(x => x.DisplayName == idProviderName);
        }
    }
}
