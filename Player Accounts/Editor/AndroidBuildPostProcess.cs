#if UNITY_ANDROID
using System;
using System.IO;
using System.Xml;
using UnityEditor.Android;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts.Editor
{
    class AndroidBuildPostProcess : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 1;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            var settings = SettingsUtils.LoadSettings();

            if (settings == null ||
                string.IsNullOrEmpty(settings.DeepLinkUriHostPrefix) ||
                string.IsNullOrEmpty(settings.DeepLinkUriScheme))
            {
                return;
            }

            Logger.Log("AndroidBuildPostProcess: Adding deeplink intent for player login postback to AndroidManifest.xml");
            var manifestFilename = JoinPaths(new[] { path, "src", "main", "AndroidManifest.xml" });

            var document = new XmlDocument();
            document.Load(manifestFilename);

            var nsmgr = new XmlNamespaceManager(document.NameTable);
            nsmgr.AddNamespace("android", "http://schemas.android.com/apk/res/android");

            if (document.DocumentElement == null)
            {
                Debug.LogWarning("<PlayerAccounts> AndroidBuildPostProcess: Could not load AndroidManifest.xml");
                return;
            }

            XmlNode mainActivityNode = null;

            var mainActivityIntentNode = document.DocumentElement.SelectSingleNode(
                "application/activity/intent-filter[action[@android:name=\"android.intent.action.MAIN\"] and category[@android:name=\"android.intent.category.LAUNCHER\"]]",
                nsmgr);
            if (mainActivityIntentNode != null)
            {
                mainActivityNode = mainActivityIntentNode.ParentNode;
            }

            if (mainActivityNode?.OwnerDocument == null)
            {
                Debug.LogWarning("<PlayerAccounts> AndroidBuildPostProcess: AndroidManifest.xml: Could not find the main activity");
                return;
            }

            var intentNodes = mainActivityNode.SelectNodes(
                $"intent-filter[action[@android:name=\"android.intent.action.VIEW\"] and data[starts-with(@android:host, '{settings.DeepLinkUriHostPrefix}')]]",
                nsmgr);

            if (intentNodes?.Count > 0)
            {
                foreach (XmlNode node in intentNodes)
                {
                    mainActivityNode.RemoveChild(node);
                }
            }

            var uriHost = settings.UseCustomUri ? settings.DeepLinkUriHostPrefix : settings.DeepLinkUriHostPrefix + Application.cloudProjectId;
            mainActivityNode.AppendChild(mainActivityNode.OwnerDocument.ImportNode(BuildeNode($@"
                <intent-filter  xmlns:android=""http://schemas.android.com/apk/res/android"">
                <action android:name=""android.intent.action.VIEW"" />
                <category android:name=""android.intent.category.DEFAULT"" />
                <category android:name= ""android.intent.category.BROWSABLE"" />
                <data android:scheme=""{settings.DeepLinkUriScheme}"" android:host=""{uriHost}"" />
                </intent-filter>"), true));

            document.Save(manifestFilename);
        }

        XmlNode BuildeNode(string text)
        {
            var doc = new XmlDocument();
            doc.LoadXml(text);

            return doc.DocumentElement;
        }

        string JoinPaths(string[] parts)
        {
            var path = "";
            foreach (var part in parts)
            {
                path = Path.Combine(path, part);
            }

            return path;
        }
    }
}
#endif
