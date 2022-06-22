using System;
using Unity.Services.Core.Editor.OrganizationHandler;
using UnityEditor;

namespace Unity.Services.Authentication.Editor
{
    static class AuthenticationAdminClientManager
    {
        const string k_CloudEnvironmentArg = "-cloudEnvironment";
        const string k_StagingEnvironment = "staging";

        internal static IAuthenticationAdminClient Create()
        {
            if (!IsConfigured())
            {
                return null;
            }

            var host = GetHost(GetCloudEnvironment(Environment.GetCommandLineArgs()));
            var networkClient = new AuthenticationAdminNetworkClient(host, GetOrganizationId(), GetProjectId(), new NetworkHandler());
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

        internal static string GetProjectId()
        {
            return CloudProjectSettings.projectId;
        }

        static string GetHost(string cloudEnvironment)
        {
            switch (cloudEnvironment)
            {
                case k_StagingEnvironment:
                    return "https://staging.services.unity.com";
                default:
                    return "https://services.unity.com";
            }
        }

        internal static string GetCloudEnvironment(string[] commandLineArgs)
        {
            try
            {
                var cloudEnvironmentIndex = Array.IndexOf(commandLineArgs, k_CloudEnvironmentArg);

                if (cloudEnvironmentIndex >= 0 && cloudEnvironmentIndex <= commandLineArgs.Length - 2)
                {
                    return commandLineArgs[cloudEnvironmentIndex + 1];
                }
            }
            catch (Exception e)
            {
                Logger.LogVerbose(e);
            }

            return null;
        }
    }
}
