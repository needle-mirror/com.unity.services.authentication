using System.Collections.Generic;
using Unity.Services.Core.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Services.Authentication.PlayerAccounts.Editor
{
    class PlayerAccountSettingsProvider : EditorGameServiceSettingsProvider
    {
        const string k_Title = "Unity Player Accounts";

        protected override IEditorGameService EditorGameService  => EditorGameServiceRegistry.Instance.GetEditorGameService<PlayerAccountIdentifier>();
        protected override string Title => k_Title;

        protected override string Description => "Unity Player Accounts is Unity’s comprehensive sign-in solution that supports persistence across games, devices and platforms.\n\n" +
        "Unity Player Accounts must be configured as an identity provider in the dashboard to be used.\n\n" +
        "Once setup, your client id will automatically synchronize when refreshing identity providers in the editor.";

        PlayerAccountSettingsProvider(SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(GenerateProjectSettingsPath(k_Title), scopes, keywords)
        {
        }

        protected override VisualElement GenerateServiceDetailUI()
        {
            return new PlayerAccountSettingsElement(SettingsUtils.LoadOrCreateSettings());
        }

        protected override VisualElement GenerateUnsupportedDetailUI()
        {
            return GenerateServiceDetailUI();
        }

        /// <summary>
        /// Method which adds your settings provider to ProjectSettings
        /// </summary>
        /// <returns>A <see cref="PlayerAccountSettingsProvider"/>.</returns>
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new PlayerAccountSettingsProvider(SettingsScope.Project);
        }
    }
}
