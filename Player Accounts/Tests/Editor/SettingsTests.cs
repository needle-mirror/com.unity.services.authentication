using NUnit.Framework;
using Unity.Services.Authentication.PlayerAccounts.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts.EditorTests
{
    public class SettingsTests
    {
        const string k_AssetPath = "Assets/Resources";

        [Test]
        public void OpenSettings_OpensSettingsAsset()
        {
            // Arrange
            if (!AssetDatabase.IsValidFolder(k_AssetPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            var assetPath = $"Assets/Resources/{nameof(UnityPlayerAccountSettings)}.asset";

            var settings = ScriptableObject.CreateInstance<UnityPlayerAccountSettings>();
            AssetDatabase.CreateAsset(settings, assetPath);

            // Act
            SettingsUtils.LoadSettings();

            // Assert
            Assert.IsTrue(AssetDatabase.IsOpenForEdit(assetPath), "Settings asset should be open for edit");
        }
    }
}
