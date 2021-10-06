using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Services.Authentication.Editor
{
    class ConfigureProjectElement : VisualElement
    {
        internal const string k_Uxml = "Packages/com.unity.services.authentication/Editor/UXML/ConfigureProject.uxml";
        internal const string k_Uss = "Packages/com.unity.services.authentication/Editor/USS/ConfigureProjectStyleSheet.uss";

        public ConfigureProjectElement()
        {
            var containerAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_Uxml);
            if (containerAsset != null)
            {
                var containerUI = containerAsset.CloneTree().contentContainer;

                var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(k_Uss);
                if (styleSheet != null)
                {
                    containerUI.styleSheets.Add(styleSheet);
                }
                else
                {
                    throw new Exception("Asset not found: " + k_Uss);
                }

                containerUI.Q<Button>("configure-project").clicked += ConfigureProject;

                Add(containerUI);
            }
            else
            {
                throw new Exception("Asset not found: " + k_Uxml);
            }
        }

        void ConfigureProject()
        {
#if UNITY_2019
            EditorApplication.ExecuteMenuItem("Window/General/Services");
#else
            SettingsService.OpenProjectSettings("Project/Services");
#endif
        }
    }
}
