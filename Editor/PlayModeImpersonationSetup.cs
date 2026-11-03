#if FEATURE_LIVE_RELEASES
using System;
using Unity.Services.Authentication.Internal;
using Unity.Services.Core;
using Unity.Services.Core.Editor;
using Unity.Services.Core.Editor.Releases;
using UnityEditor;

namespace Unity.Services.Authentication.Editor
{
    static class PlayModeImpersonationSetup
    {
        [InitializeOnLoadMethod]
        static void SubscribeToResetComplete()
        {
            CoreResetter.OnResetComplete -= Register;
            CoreResetter.OnResetComplete += Register;
        }

        static void Register()
        {
            try
            {
                if (ReleaseConfigurationStatus.IsReleaseConfiguredForCurrentEnvironment())
                {
                    // Pre-fetch the token so it is in-flight by the time the first auth
                    // request runs. The provider calls GetServicesGatewayTokenAsync() fresh
                    // each time so that refresh and retry logic inside AccessTokens runs
                    // correctly on subsequent play-mode sessions.
                    var accessTokens = new AccessTokens();
                    _ = accessTokens.GetServicesGatewayTokenAsync();
                    PlayModeImpersonationBridge.GenesisTokenProvider = () => accessTokens.GetServicesGatewayTokenAsync();
                }
                else
                {
                    PlayModeImpersonationBridge.GenesisTokenProvider = null;
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[Impersonation] Could not read release settings; play mode impersonation will not be active. Make sure you have configured a release for the current environment in Project Settings > Services. ({e.Message})");
                PlayModeImpersonationBridge.GenesisTokenProvider = null;
            }
        }
    }
}
#endif
