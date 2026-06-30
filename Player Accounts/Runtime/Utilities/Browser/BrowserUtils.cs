using System;
using Unity.Services.Core.Configuration.Internal;

namespace Unity.Services.Authentication.PlayerAccounts
{
    internal static class BrowserUtils
    {
        internal static IBrowserUtils CreateBrowserUtils(ICloudProjectId cloudProjectId, UnityPlayerAccountSettings settings, Action<string> onAuthCodeReceived, PlayerAccountServiceInternal accountService)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            var standaloneBrowserUtils = new StandaloneBrowserUtils();
            standaloneBrowserUtils.AuthCodeReceivedEvent += onAuthCodeReceived;
            return standaloneBrowserUtils;
#elif UNITY_ANDROID
            return new AndroidBrowserUtils(cloudProjectId, settings);
#elif UNITY_IOS
            // iOS delivers the OAuth redirect through the ASWebAuthenticationSession completion
            // handler (callbackUrl + error), which OnWebAuthSessionCompleted routes into the
            // existing deep-link handling.
            return new IOSBrowserUtils(cloudProjectId, settings, accountService);
#else
            return null;
#endif
        }
    }
}
