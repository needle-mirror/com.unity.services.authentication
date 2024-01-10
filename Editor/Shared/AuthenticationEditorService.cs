using System;
using System.Linq;
using Unity.Services.Authentication.Generated;
using Unity.Services.Authentication.Shared;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Environments.Internal;
using Unity.Services.Core.Scheduler.Internal;
using Unity.Services.Core.Telemetry.Internal;

namespace Unity.Services.Authentication.Editor.Shared
{
    /// <summary>
    /// Authentication service usable in the editor context.
    /// </summary>
    internal static partial class AuthenticationEditorService
    {
        const string k_CloudEnvironmentArg = "-cloudEnvironment";
        const string k_StagingEnvironment = "staging";

        /// <summary>
        /// Create an instance of the authentication service usable in the editor context.
        /// </summary>
        /// <param name="profileScope">The profile scope to use for caching data</param>
        /// <param name="customScheduler">To provide a custom action scheduler. One is created by default.</param>
        /// <param name="customEnvironment">To provide a custom environment name provider. One is created by default.</param>
        /// <param name="customProjectId">To provide a custom project id provider. One is created by default.</param>
        /// <param name="customMetricsFactory">To provide a custom metrics factory. One is created by default.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// If an invalid profile scope is provided. The scope must not be null or empty.
        /// </exception>
        internal static IAuthenticationEditorService Create(
            string profileScope,
            IActionScheduler customScheduler = null,
            IEnvironments customEnvironment = null,
            ICloudProjectId customProjectId = null,
            IMetricsFactory customMetricsFactory = null)
        {
            if (string.IsNullOrEmpty(profileScope))
            {
                throw new ArgumentException("A scope must be provided for the authentication service.");
            }

            var settings = new AuthenticationSettings();
            var scheduler = customScheduler ?? new ActionScheduler();
            var environment = customEnvironment ?? new EnvironmentProvider();
            var projectId = customProjectId ?? new CloudProjectIdProvider();
            var metricsFactory = customMetricsFactory ?? new DisabledMetricsFactory();
            var profile = new ProfileComponent(profileScope);
            var metrics = new AuthenticationMetrics(metricsFactory);
            var jwtDecoder = new JwtDecoder();
            var cache = new AuthenticationCache(projectId, profile);
            var accessToken = new AccessTokenComponent();
            var environmentId = new EnvironmentIdComponent();
            var playerId = new PlayerIdComponent(cache);
            var playerName = new PlayerNameComponent(cache);
            var sessionToken = new SessionTokenComponent(cache);
            var networkConfiguration = new NetworkConfiguration();
            var networkHandler = new NetworkHandler(networkConfiguration);

            var cloudEnvironment = GetCloudEnvironment(Environment.GetCommandLineArgs());
            var host = GetPlayerAuthHost(cloudEnvironment);

            var apiClient = new AuthenticationApiClient(networkConfiguration);
            var playerNamesConfiguration = new ApiConfiguration();
            playerNamesConfiguration.BasePath = GetPlayerNamesHost(cloudEnvironment);
            var playerNamesApi = new PlayerNamesApi(apiClient, playerNamesConfiguration);

            var networkClient = new AuthenticationNetworkClient(host,
                projectId,
                environment,
                networkHandler,
                accessToken);
            var authenticationService = new AuthenticationEditorServiceInternal(
                settings,
                networkClient,
                playerNamesApi,
                profile,
                jwtDecoder,
                cache,
                scheduler,
                metrics,
                accessToken,
                environmentId,
                playerId,
                playerName,
                sessionToken,
                environment);

            return authenticationService;
        }

        static string GetPlayerAuthHost(string cloudEnvironment)
        {
            switch (cloudEnvironment)
            {
                case k_StagingEnvironment:
                    return "https://player-auth-stg.services.api.unity.com";
                default:
                    return "https://player-auth.services.api.unity.com";
            }
        }

        static string GetPlayerNamesHost(string cloudEnvironment)
        {
            switch (cloudEnvironment)
            {
                case k_StagingEnvironment:
                    return "https://social-stg.services.api.unity.com/v1";
                default:
                    return "https://social.services.api.unity.com/v1";
            }
        }

        static string GetCloudEnvironment(string[] commandLineArgs)
        {
            try
            {
                var cloudEnvironmentField = commandLineArgs.FirstOrDefault(x => x.StartsWith(k_CloudEnvironmentArg));

                if (cloudEnvironmentField != null)
                {
                    var cloudEnvironmentIndex = Array.IndexOf(commandLineArgs, cloudEnvironmentField);

                    if (cloudEnvironmentField == k_CloudEnvironmentArg)
                    {
                        if (cloudEnvironmentIndex <= commandLineArgs.Length - 2)
                        {
                            return commandLineArgs[cloudEnvironmentIndex + 1];
                        }
                    }
                    else if (cloudEnvironmentField.Contains('='))
                    {
                        var value = cloudEnvironmentField.Substring(cloudEnvironmentField.IndexOf('=') + 1);
                        return !string.IsNullOrEmpty(value) ? value : null;
                    }
                }
            }
            catch (Exception)
            {
            }

            return null;
        }
    }
}
