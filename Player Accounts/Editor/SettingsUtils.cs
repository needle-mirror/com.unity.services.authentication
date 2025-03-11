using UnityEditor;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts.Editor
{
    /// <summary>
    /// A utility class that provides functionality for creating and opening the UnityPlayerAccountSettings asset in the Unity editor.
    /// </summary>
    static class SettingsUtils
    {
        const string k_AssetPath = "Assets/Resources";
        const int k_ConfigureMenuPriority = 100;
        const string k_ServiceMenu = "Services/Unity Player Accounts/Configure";
        const string k_ProjectSettings = "Project/Services/Unity Player Accounts";

        static UnityPlayerAccountSettings Settings { get; set; }

        /// <summary>
        /// Initializes the SettingsUtils class and ensures the UnityPlayerAccountSettings asset is created.
        /// </summary>
        static SettingsUtils()
        {
        }

        /// <summary>
        /// Opens the UnityPlayerAccountSettings asset in the Unity editor.
        /// </summary>
        [MenuItem(k_ServiceMenu, priority = k_ConfigureMenuPriority)]
        public static void OpenSettingsMenu()
        {
            SettingsService.OpenProjectSettings(k_ProjectSettings);
        }

        /// <summary>
        /// Creates or retrieves the existing UnityPlayerAccountSettings asset.
        /// </summary>
        /// <returns>The UnityPlayerAccountSettings asset.</returns>
        public static UnityPlayerAccountSettings LoadSettings()
        {
            if (Settings == null)
            {
                Settings = Resources.Load<UnityPlayerAccountSettings>(nameof(UnityPlayerAccountSettings));
            }

            return Settings;
        }

        /// <summary>
        /// Creates or retrieves the existing UnityPlayerAccountSettings asset.
        /// </summary>
        /// <returns>The UnityPlayerAccountSettings asset.</returns>
        public static UnityPlayerAccountSettings LoadOrCreateSettings()
        {
            Settings = LoadSettings();

            if (Settings == null)
            {
                if (!AssetDatabase.IsValidFolder(k_AssetPath))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }

                Settings = ScriptableObject.CreateInstance<UnityPlayerAccountSettings>();
                AssetDatabase.CreateAsset(Settings, $"{k_AssetPath}/{nameof(UnityPlayerAccountSettings)}.asset");
            }

            return Settings;
        }
    }
}
