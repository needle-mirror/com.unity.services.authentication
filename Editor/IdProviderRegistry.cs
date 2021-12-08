using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Services.Authentication.Editor
{
    /// <summary>
    /// The ID provider additional custom settings element.
    /// The UI element renders after the ID Provider Element's Save/Cancel/Delete buttons.
    /// It's only enabled after the ID provider is saved, and allows additional settings to be saved separately.
    /// It must implement several events in order for the <see cref="IdProviderElement"/> to hook up and update status.
    /// </summary>
    public abstract class IdProviderCustomSettingsElement : VisualElement
    {
        /// <summary>
        /// Event triggered when the <see cref="IdProviderCustomSettingsElement"/> starts or finishes waiting for a task.
        /// The first parameter of the callback is the sender.
        /// The second parameter is true if it starts waiting, and false if it finishes waiting.
        /// </summary>
        public abstract event Action<IdProviderCustomSettingsElement, bool> Waiting;

        /// <summary>
        /// Event triggered when the current <see cref="IdProviderCustomSettingsElement"/> catches an error.
        /// The first parameter of the callback is the sender.
        /// The second parameter is the exception caught by the element.
        /// </summary>
        public abstract event Action<IdProviderCustomSettingsElement, Exception> Error;

        /// <summary>
        /// The property to get a service gateway token.
        /// </summary>
        public string GatewayToken => m_GatewayTokenCallback.Invoke();

        /// <summary>
        /// The callback to get the service gateway token
        /// </summary>
        protected Func<string> m_GatewayTokenCallback;

        /// <summary>
        /// The constructor of the IdProviderCustomSettingsElement.
        /// </summary>
        /// <param name="servicesGatewayTokenCallback">
        /// The callback action to get the service gateway token. It makes sure the token is up to date.
        /// </param>
        protected IdProviderCustomSettingsElement(Func<string> servicesGatewayTokenCallback)
        {
            m_GatewayTokenCallback = servicesGatewayTokenCallback;
        }

        /// <summary>
        /// The method for the custom settings section to refresh itself from the server side.
        /// This is called when creating IdProviders that are already created on the server side or
        /// when there is any status change on the ID provider.
        /// </summary>
        public abstract void Refresh();
    }

    /// <summary>
    /// The metadata about an ID provider that is used to render the settings UI.
    /// </summary>
    public class IdProviderOptions
    {
        /// <summary>
        /// The type of the ID provider. This is the type string that is accepted by ID Provider admin API.
        /// </summary>
        public string IdProviderType { get; set; }

        /// <summary>
        /// The display name of the ID provider.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The display name of the Client ID field. In some ID providers it can be named differently, like "App ID".
        /// </summary>
        public string ClientIdDisplayName { get; set; } = "Client ID";

        /// <summary>
        /// The display name of the Client Secret field. In some ID providers it can be named differently, like "App Secret".
        /// </summary>
        public string ClientSecretDisplayName { get; set; } = "Client Secret";

        /// <summary>
        /// Whether the client secret is needed in the target ID provider.
        /// </summary>
        public bool NeedClientSecret { get; set; }

        /// <summary>
        /// The delegate to create custom settings UI element for the ID provider.
        /// </summary>
        /// <param name="idDomain">The ID domain</param>
        /// <param name="servicesGatewayTokenCallback">
        /// The callback action to get the service gateway token. It makes sure the token is up to date.
        /// </param>
        /// <param name="skipConfirmation">Whether or not to skip the UI confirmation.</param>
        /// <returns>The additional ID provider settings element.</returns>
        public delegate IdProviderCustomSettingsElement CreateCustomSettingsElementDelegate(string idDomain, Func<string> servicesGatewayTokenCallback, bool skipConfirmation);

        /// <summary>
        /// The delegate to create custom settings UI element for the ID provider.
        /// If provided, the element is appended to the IdProviderElement.
        /// </summary>
        public CreateCustomSettingsElementDelegate CustomSettingsElementCreator { get; set; }
    }

    /// <summary>
    /// The registry for ID providers.
    /// </summary>
    public static class IdProviderRegistry
    {
        const string k_IdProviderApple = "apple.com";
        const string k_IdProviderGoogle = "google.com";
        const string k_IdProviderFacebook = "facebook.com";
        const string k_IdProviderSteam = "steampowered.com";

        static Dictionary<string, IdProviderOptions> s_IdProviderOptions = DefaultIdProviderOptions;

        /// <summary>
        /// Expose the internal ID provider options for testing purpose.
        /// </summary>
        internal static Dictionary<string, IdProviderOptions> IdProviderOptions
        {
            get => s_IdProviderOptions;
            set
            {
                s_IdProviderOptions = value;
                s_SortedKeys = GetSortedKeys();
            }
        }

        /// <summary>
        /// Reset the registry to use defaults.
        /// </summary>
        internal static void Reset()
        {
            IdProviderOptions = DefaultIdProviderOptions;
        }

        /// <summary>
        /// The default set of ID provider options.
        /// </summary>
        static Dictionary<string, IdProviderOptions> DefaultIdProviderOptions =>
            new Dictionary<string, IdProviderOptions>
        {
            [k_IdProviderApple] = new IdProviderOptions
            {
                IdProviderType = k_IdProviderApple,
                DisplayName = "Sign-in with Apple",
                ClientIdDisplayName = "App ID",
                NeedClientSecret = false
            },
            [k_IdProviderGoogle] = new IdProviderOptions
            {
                IdProviderType = k_IdProviderGoogle,
                DisplayName = "Google",
                ClientIdDisplayName = "Client ID",
                NeedClientSecret = false
            },
            [k_IdProviderFacebook] = new IdProviderOptions
            {
                IdProviderType = k_IdProviderFacebook,
                DisplayName = "Facebook",
                ClientIdDisplayName = "App ID",
                ClientSecretDisplayName = "App Secret",
                NeedClientSecret = true
            },
            [k_IdProviderSteam] = new IdProviderOptions
            {
                IdProviderType = k_IdProviderSteam,
                DisplayName = "Steam",
                ClientIdDisplayName = "App ID",
                ClientSecretDisplayName = "Key",
                NeedClientSecret = true
            }
        };

        static string[] s_SortedKeys = GetSortedKeys();

        /// <summary>
        /// All ID provider types, sorted by alphabetical order.
        /// </summary>
        public static IEnumerable<string> AllTypes => s_SortedKeys;

        /// <summary>
        /// Register a new ID provider option.
        /// </summary>
        /// <param name="idProviderOptions">The new ID provider option to register.</param>
        public static void Register(IdProviderOptions idProviderOptions)
        {
            s_IdProviderOptions[idProviderOptions.IdProviderType] = idProviderOptions;
            s_SortedKeys = GetSortedKeys();
        }

        /// <summary>
        /// Unregister an ID provider option.
        /// </summary>
        /// <param name="idProviderType">The ID provider option to unregister.</param>
        public static void Unregister(string idProviderType)
        {
            s_IdProviderOptions.Remove(idProviderType);
            s_SortedKeys = GetSortedKeys();
        }

        /// <summary>
        /// Get an ID provider option by type.
        /// </summary>
        /// <param name="idProviderType">The ID provider type.</param>
        /// <returns>The ID provider option.</returns>
        public static IdProviderOptions GetOptions(string idProviderType)
        {
            if (!s_IdProviderOptions.ContainsKey(idProviderType))
            {
                return null;
            }
            return s_IdProviderOptions[idProviderType];
        }

        static string[] GetSortedKeys()
        {
            return s_IdProviderOptions.Keys.OrderBy(s => s).ToArray();
        }
    }
}
