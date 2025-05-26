using Unity.Services.Authentication.Components;
using UnityEditor;
using UnityEngine;

namespace Unity.Services.Authentication.Editor
{
    class PlayerAuthenticationEditor
    {
        [MenuItem("CONTEXT/PlayerAuthentication/Open Authentication Settings", false, 2003)]
        static void OpenAuthenticationSettings(MenuCommand _)
        {
            SettingsService.OpenProjectSettings("Project/Services/Authentication");
        }
    }
}
