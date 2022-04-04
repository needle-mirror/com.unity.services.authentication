using System;
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
        /// <param name="gatewayTokenCallback">
        /// The callback action to get the service gateway token. It makes sure the token is up to date.
        /// </param>
        protected IdProviderCustomSettingsElement(Func<string> gatewayTokenCallback)
        {
            m_GatewayTokenCallback = gatewayTokenCallback;
        }

        /// <summary>
        /// The method for the custom settings section to refresh itself from the server side.
        /// This is called when creating IdProviders that are already created on the server side or
        /// when there is any status change on the ID provider.
        /// </summary>
        public abstract void Refresh();
    }
}
