using System.Threading.Tasks;
using Unity.Services.Authentication.Generated;
using Unity.Services.Authentication.Internal;
using Unity.Services.Authentication.Shared;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Environments.Internal;
using Unity.Services.Core.Internal;
using Unity.Services.Core.Scheduler.Internal;
using Unity.Services.Core.Telemetry.Internal;
using UnityEngine;

namespace Unity.Services.Authentication
{
    class AuthenticationPackageInitializer : IInitializablePackage
    {
        const string k_CloudEnvironmentKey = "com.unity.services.core.cloud-environment";
        const string k_StagingEnvironment = "staging";

        public Task Initialize(CoreRegistry registry)
        {
            var settings = new AuthenticationSettings();
            var scheduler = registry.GetServiceComponent<IActionScheduler>();
            var environment = registry.GetServiceComponent<IEnvironments>();
            var projectId = registry.GetServiceComponent<ICloudProjectId>();
            var projectConfiguration = registry.GetServiceComponent<IProjectConfiguration>();
            var profile = new ProfileComponent(projectConfiguration.GetString(AuthenticationExtensions.ProfileKey, "default"));
            var metricsFactory = registry.GetServiceComponent<IMetricsFactory>();
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

            var host = GetPlayerAuthHost(projectConfiguration);

            var apiClient = new AuthenticationApiClient(networkConfiguration);
            var playerNamesConfiguration = new ApiConfiguration();
            playerNamesConfiguration.BasePath = GetPlayerNamesHost(projectConfiguration);
            var playerNamesApi = new PlayerNamesApi(apiClient, playerNamesConfiguration);

            var networkClient = new AuthenticationNetworkClient(host,
                projectId,
                environment,
                networkHandler,
                accessToken);
            var authenticationService = new AuthenticationServiceInternal(
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
                sessionToken);

            AuthenticationService.Instance = authenticationService;
            registry.RegisterServiceComponent<IAccessToken>(authenticationService.AccessTokenComponent);
            registry.RegisterServiceComponent<IEnvironmentId>(authenticationService.EnvironmentIdComponent);
            registry.RegisterServiceComponent<IPlayerId>(authenticationService.PlayerIdComponent);

            return Task.CompletedTask;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            CoreRegistry.Instance.RegisterPackage(new AuthenticationPackageInitializer())
                .DependsOn<IEnvironments>()
                .DependsOn<IActionScheduler>()
                .DependsOn<ICloudProjectId>()
                .DependsOn<IProjectConfiguration>()
                .DependsOn<IMetricsFactory>()
                .ProvidesComponent<IPlayerId>()
                .ProvidesComponent<IAccessToken>()
                .ProvidesComponent<IEnvironmentId>();
        }

        string GetPlayerAuthHost(IProjectConfiguration projectConfiguration)
        {
            var cloudEnvironment = projectConfiguration?.GetString(k_CloudEnvironmentKey);

            switch (cloudEnvironment)
            {
                case k_StagingEnvironment:
                    return "https://player-auth-stg.services.api.unity.com";
                default:
                    return "https://player-auth.services.api.unity.com";
            }
        }

        string GetPlayerNamesHost(IProjectConfiguration projectConfiguration)
        {
            var cloudEnvironment = projectConfiguration?.GetString(k_CloudEnvironmentKey);

            switch (cloudEnvironment)
            {
                case k_StagingEnvironment:
                    return "https://social-stg.services.api.unity.com/v1";
                default:
                    return "https://social.services.api.unity.com/v1";
            }
        }
    }
}
