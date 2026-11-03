#if FEATURE_LIVE_RELEASES
#if UNITY_EDITOR
using System;
using System.Threading.Tasks;

namespace Unity.Services.Authentication.Internal
{
    static class PlayModeImpersonationBridge
    {
        internal static Func<Task<string>> GenesisTokenProvider;
    }
}
#endif
#endif
