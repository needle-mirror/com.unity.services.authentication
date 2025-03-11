using UnityEditor;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts.Editor
{
    [CustomEditor(typeof(UnityPlayerAccountSettings))]
    class PlayerAccountSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw default inspector
            DrawDefaultInspector();

            if (GUILayout.Button("Open Settings"))
            {
                SettingsUtils.OpenSettingsMenu();
            }
        }
    }
}
