using System.Threading.Tasks;
using Unity.Services.Authentication.Internal;
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
#if UNITY_SERVICES_STAGING || AUTHENTICATION_TESTING_STAGING_UAS
        const string k_UasHost = "https://player-auth-stg.services.api.unity.com";
#else
        const string k_UasHost = "https://player-auth.services.api.unity.com";
#endif

        public Task Initialize(CoreRegistry registry)
        {
            var settings = new AuthenticationSettings();
            var scheduler = registry.GetServiceComponent<IActionScheduler>();
            var environment = registry.GetServiceComponent<IEnvironments>();
            var projectId = registry.GetServiceComponent<ICloudProjectId>();
            var projectConfiguration = registry.GetServiceComponent<IProjectConfiguration>();
            var profile = new ProfileComponent(projectConfiguration.GetString(AuthenticationExtensions.ProfileKey, "default"));
            var dateTime = new DateTimeWrapper();
            var metricsFactory = registry.GetServiceComponent<IMetricsFactory>();
            var metrics = new AuthenticationMetrics(metricsFactory);
            var jwtDecoder = new JwtDecoder(dateTime);
            var cache = new AuthenticationCache(projectId, profile);
            var accessToken = new AccessTokenComponent();
            var environmentId = new EnvironmentIdComponent();
            var playerId = new PlayerIdComponent(cache);
            var sessionToken = new SessionTokenComponent(cache);
            var wellKnownKeys = new WellKnownKeysComponent();
            var networkHandler = new NetworkHandler();
            var networkClient = new AuthenticationNetworkClient(k_UasHost,
                projectId,
                environment,
                networkHandler,
                accessToken);
            var authenticationService = new AuthenticationServiceInternal(
                settings,
                networkClient,
                profile,
                jwtDecoder,
                cache,
                scheduler,
                dateTime,
                metrics,
                accessToken,
                environmentId,
                playerId,
                sessionToken,
                wellKnownKeys);

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
    }
}
