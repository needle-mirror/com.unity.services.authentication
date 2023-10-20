using System;
using Unity.Services.Core.Configuration.Internal;

namespace Unity.Services.Authentication.PlayerAccounts
{
    internal static class BrowserUtils
    {
        internal static IBrowserUtils CreateBrowserUtils(ICloudProjectId cloudProjectId, UnityPlayerAccountSettings settings, Action<string> onAuthCodeReceived)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            var standaloneBrowserUtils = new StandaloneBrowserUtils();
            standaloneBrowserUtils.AuthCodeReceivedEvent += onAuthCodeReceived;
            return standaloneBrowserUtils;
#elif UNITY_ANDROID
            return new AndroidBrowserUtils(cloudProjectId, settings);
#elif UNITY_IOS
            return new IOSBrowserUtils(cloudProjectId, settings);
#else
            return null;
#endif
        }
    }
}
