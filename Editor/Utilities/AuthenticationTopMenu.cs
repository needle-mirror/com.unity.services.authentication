using UnityEditor;

namespace Unity.Services.Authentication.Editor
{
    static class AuthenticationTopMenu
    {
        const int k_ConfigureMenuPriority = 100;
        const string k_ServiceMenuRoot = "Services/Authentication/";

        [MenuItem(k_ServiceMenuRoot + "Configure", priority = k_ConfigureMenuPriority)]
        static void ShowProjectSettings()
        {
            AuthenticationEditorAnalytics.SendTopMenuConfigureEvent();
            SettingsService.OpenProjectSettings("Project/Services/Authentication");
        }
    }
}
