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
            var networkConfiguration = new NetworkConfiguration();
            var networkHandler = new NetworkHandler(networkConfiguration);
            var networkClient = new AuthenticationAdminNetworkClient(host, GetProjectId(), networkHandler);
            return new AuthenticationAdminClient(networkClient, new GenesisTokenProvider());
        }

        /// <summary>
        /// Creates a client that manages Unity Player Accounts for the linked project, or returns null when the project
        /// is not linked to a Unity Cloud project.
        /// </summary>
        internal static IPlayerAccountsAdminClient CreatePlayerAccountsAdminClient()
        {
            if (!IsConfigured())
            {
                return null;
            }

            var host = GetHost(GetCloudEnvironment(Environment.GetCommandLineArgs()));
            var networkHandler = new NetworkHandler(new NetworkConfiguration());
            var authenticationAdmin = new AuthenticationAdminNetworkClient(host, GetProjectId(), networkHandler);
            return new PlayerAccountsAdminClient(host, GetProjectId(), networkHandler, authenticationAdmin, new GenesisTokenProvider());
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
