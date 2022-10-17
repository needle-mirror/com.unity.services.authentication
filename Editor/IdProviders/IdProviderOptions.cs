using System;

namespace Unity.Services.Authentication.Editor
{
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
        /// The OpenID Connect Id provider configuration struct
        /// </summary>
        internal OpenIDConfig OidcConfig { get; set; }

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
}
