using Unity.Services.Core.Editor.OrganizationHandler;
using UnityEditor;

namespace Unity.Services.Authentication.Editor
{
    static class AuthenticationAdminClientManager
    {
#if UNITY_SERVICES_STAGING || AUTHENTICATION_TESTING_STAGING_UAS
        const string k_ServicesHost = "https://staging.services.unity.com";
#else
        const string k_ServicesHost = "https://services.unity.com";
#endif

        internal static IAuthenticationAdminClient Create()
        {
            if (!IsConfigured())
            {
                return null;
            }

            var networkClient = new AuthenticationAdminNetworkClient(k_ServicesHost, GetOrganizationId(), GetProjectId(), new NetworkHandler());
            return new AuthenticationAdminClient(networkClient, new GenesisTokenProvider());
        }

        internal static bool IsConfigured()
        {
            return !string.IsNullOrEmpty(GetOrganizationId()) && !string.IsNullOrEmpty(GetProjectId());
        }

        internal static string GetOrganizationId()
        {
            return OrganizationProvider.Organization.Key;
        }

        static string GetProjectId()
        {
            return CloudProjectSettings.projectId;
        }
    }
}
