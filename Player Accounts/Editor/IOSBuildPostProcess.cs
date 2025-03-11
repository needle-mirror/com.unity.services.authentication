#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts.Editor
{
    internal class IOSBuildPostProcess
    {
        // Runs all the post process build steps. Called from Unity during build
        [PostProcessBuild(0)] // Configures this this post process to run first
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            var settings = SettingsUtils.LoadSettings();

            if (settings == null ||
                string.IsNullOrEmpty(settings.DeepLinkUriHostPrefix) ||
                string.IsNullOrEmpty(settings.DeepLinkUriScheme))
            {
                return;
            }

            var infoPlist = new UnityEditor.iOS.Xcode.PlistDocument();
            var infoPlistPath = pathToBuiltProject + "/Info.plist";
            infoPlist.ReadFromFile(infoPlistPath);

            var uriHost = settings.UseCustomUri ? settings.DeepLinkUriHostPrefix : settings.DeepLinkUriHostPrefix + Application.cloudProjectId;

            // Register ios URL scheme for external apps to launch this app.
            var urlTypeDict = infoPlist.root.CreateArray("CFBundleURLTypes").AddDict();
            urlTypeDict.SetString("CFBundleURLName", $"{uriHost}");

            var urlSchemes = urlTypeDict.CreateArray("CFBundleURLSchemes");
            urlSchemes.AddString(settings.DeepLinkUriScheme);

            infoPlist.WriteToFile(infoPlistPath);
        }
    }
}
#endif
