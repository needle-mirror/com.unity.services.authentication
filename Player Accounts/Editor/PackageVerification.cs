#if WARN_UNINSTALL_UPA
using UnityEditor;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts.Editor
{
    static class PackageVerification
    {
        [InitializeOnLoadMethod]
        public static void Warn()
        {
            Debug.LogWarning("The Unity Player Accounts feature was merged into the Authentication package. You can uninstall the 'Player Accounts' package.");
        }
    }
}
#endif
