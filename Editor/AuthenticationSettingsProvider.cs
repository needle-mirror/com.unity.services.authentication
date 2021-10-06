using System.Collections.Generic;
using Unity.Services.Core.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Services.Authentication.Editor
{
    class AuthenticationSettingsProvider : EditorGameServiceSettingsProvider
    {
        const string k_Title = "Authentication";

        /// <summary>
        /// Accessor for the operate service
        /// Used to toggle and get dashboard access
        /// </summary>
        protected override IEditorGameService EditorGameService => EditorGameServiceRegistry.Instance.GetEditorGameService<AuthenticationIdentifier>();

        /// <summary>
        /// Title shown in the header for the project settings
        /// </summary>
        protected override string Title => k_Title;

        /// <summary>
        /// Description show in the header for the project settings
        /// </summary>
        protected override string Description => "This package provides a system for working with the Unity User Authentication Service (UAS), including log-in, player ID and access token retrieval, and session persistence.";

        AuthenticationSettingsProvider(SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(GenerateProjectSettingsPath(k_Title), scopes, keywords) {}

        /// <inheritdoc/>
        protected override VisualElement GenerateServiceDetailUI()
        {
            if (!AuthenticationAdminClientManager.IsConfigured())
            {
                return new ConfigureProjectElement();
            }

            var adminClient = AuthenticationAdminClientManager.Create();
            var settingsElement = new AuthenticationSettingsElement(adminClient, CloudProjectSettings.projectId);
            settingsElement.RefreshIdProviders();
            return settingsElement;
        }

        /// <inheritdoc/>
        protected override VisualElement GenerateUnsupportedDetailUI()
        {
            return GenerateServiceDetailUI();
        }

        /// <summary>
        /// Method which adds your settings provider to ProjectSettings
        /// </summary>
        /// <returns>A <see cref="AuthenticationSettingsProvider"/>.</returns>
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new AuthenticationSettingsProvider(SettingsScope.Project);
        }
    }
}
