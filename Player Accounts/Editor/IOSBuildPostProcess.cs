#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts.Editor
{
    internal class IOSBuildPostProcess
    {
        const string k_CFBundleURLTypes = "CFBundleURLTypes";
        const string k_CFBundleURLName = "CFBundleURLName";
        const string k_CFBundleURLSchemes = "CFBundleURLSchemes";

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

            var infoPlist = new PlistDocument();
            var infoPlistPath = pathToBuiltProject + "/Info.plist";
            infoPlist.ReadFromFile(infoPlistPath);

            var uriHost = settings.UseCustomUri ? settings.DeepLinkUriHostPrefix : settings.DeepLinkUriHostPrefix + Application.cloudProjectId;

            // Register ios URL scheme for external apps to launch this app.
            // Merge into the existing CFBundleURLTypes array so we don't strip
            // schemes registered by other packages.
            RegisterUrlScheme(infoPlist, uriHost, settings.DeepLinkUriScheme);

            infoPlist.WriteToFile(infoPlistPath);
        }

        // Additive only: ensures the (urlName, urlScheme) pair is present in
        // CFBundleURLTypes without removing any pre-existing entries or schemes.
        // Schemes registered by a previous build under our urlName are left in
        // place, so an "Append" rebuild that changes DeepLinkUriScheme will
        // accumulate stale schemes until a "Replace" build refreshes the plist.
        internal static void RegisterUrlScheme(PlistDocument plist, string urlName, string urlScheme)
        {
            var urlTypesArray = plist.root[k_CFBundleURLTypes] as PlistElementArray
                ?? plist.root.CreateArray(k_CFBundleURLTypes);

            // Reuse the entry with the same identifier if it already exists
            // (e.g. "Append" build mode re-running the post-process) so we
            // refresh its scheme list rather than appending a duplicate entry.
            PlistElementDict urlTypeDict = null;
            foreach (var element in urlTypesArray.values)
            {
                var dict = element as PlistElementDict;
                if (dict == null)
                {
                    continue;
                }

                if (!dict.values.TryGetValue(k_CFBundleURLName, out var nameElement))
                {
                    continue;
                }

                var nameString = nameElement as PlistElementString;
                if (nameString != null && nameString.value == urlName)
                {
                    urlTypeDict = dict;
                    break;
                }
            }

            if (urlTypeDict == null)
            {
                urlTypeDict = urlTypesArray.AddDict();
                urlTypeDict.SetString(k_CFBundleURLName, urlName);
            }

            var urlSchemes = urlTypeDict[k_CFBundleURLSchemes] as PlistElementArray
                ?? urlTypeDict.CreateArray(k_CFBundleURLSchemes);

            foreach (var schemeElement in urlSchemes.values)
            {
                var schemeString = schemeElement as PlistElementString;
                if (schemeString != null && schemeString.value == urlScheme)
                {
                    return;
                }
            }

            urlSchemes.AddString(urlScheme);
        }
    }
}
#endif
