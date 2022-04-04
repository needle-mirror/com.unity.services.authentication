using System.Collections.Generic;
using Unity.Services.Core.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Services.Authentication.Editor
{
    class AuthenticationSettingsProvider : EditorGameServiceSettingsProvider
    {
        const string k_Title = "Authentication";

        static AuthenticationSettingsElement Element { get; set; }
        static string CloudProjectId { get; set; }

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
        protected override string Description => "This service provides player authentication for Unity Gaming Services.";

        AuthenticationSettingsProvider(SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(GenerateProjectSettingsPath(k_Title), scopes, keywords)
        {}

        /// <inheritdoc/>
        protected override VisualElement GenerateServiceDetailUI()
        {
            var cloudProjectId = CloudProjectSettings.projectId;

            if (Element == null || CloudProjectId != cloudProjectId)
            {
                Element = new AuthenticationSettingsElement(AuthenticationAdminClientManager.Create());
                CloudProjectId = CloudProjectSettings.projectId;
                Element.RefreshIdProviders();
            }

            return Element;
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
            AuthenticationEditorAnalytics.SendProjectSettingsToolEvent();
            return new AuthenticationSettingsProvider(SettingsScope.Project);
        }
    }
}
