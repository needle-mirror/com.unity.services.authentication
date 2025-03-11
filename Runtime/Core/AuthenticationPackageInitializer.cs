using System;
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
    class AuthenticationPackageInitializer : IInitializablePackageV2
    {
        const string k_CloudEnvironmentKey = "com.unity.services.core.cloud-environment";
        const string k_StagingEnvironment = "staging";
        const string k_DefaultProfile = "default";
        const string k_EditorModeArg = "-editor-mode";
        const string k_NameArg = "-name";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitializeOnLoad()
        {
            var initializer = new AuthenticationPackageInitializer();
            initializer.Register(CorePackageRegistry.Instance);
        }

        public void Register(CorePackageRegistry registry)
        {
            registry.Register(this)
                .DependsOn<IEnvironments>()
                .DependsOn<IActionScheduler>()
                .DependsOn<ICloudProjectId>()
                .DependsOn<IProjectConfiguration>()
                .DependsOn<IMetricsFactory>()
                .ProvidesComponent<IPlayerId>()
                .ProvidesComponent<IAccessToken>()
                .ProvidesComponent<IAccessTokenObserver>()
                .ProvidesComponent<IEnvironmentId>();
        }

        public Task Initialize(CoreRegistry registry)
        {
            AuthenticationService.Instance = InitializeService(registry);
            return Task.CompletedTask;
        }

        public Task InitializeInstanceAsync(CoreRegistry registry)
        {
            InitializeService(registry);
            return Task.CompletedTask;
        }

        AuthenticationServiceInternal InitializeService(CoreRegistry registry)
        {
            var settings = new AuthenticationSettings();
            var scheduler = registry.GetServiceComponent<IActionScheduler>();
            var environment = registry.GetServiceComponent<IEnvironments>();
            var projectId = registry.GetServiceComponent<ICloudProjectId>();
            var projectConfiguration = registry.GetServiceComponent<IProjectConfiguration>();
            var profile = new ProfileComponent(GetProfile(projectConfiguration));
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
                sessionToken,
                environment);

            registry.RegisterService<IAuthenticationService>(authenticationService);
            registry.RegisterServiceComponent<IAccessToken>(authenticationService.AccessTokenComponent);
            registry.RegisterServiceComponent<IAccessTokenObserver>(authenticationService.AccessTokenComponent);
            registry.RegisterServiceComponent<IEnvironmentId>(authenticationService.EnvironmentIdComponent);
            registry.RegisterServiceComponent<IPlayerId>(authenticationService.PlayerIdComponent);

            return authenticationService;
        }

        string GetProfile(IProjectConfiguration projectConfiguration)
        {
            var profile = projectConfiguration.GetString(AuthenticationExtensions.ProfileKey, k_DefaultProfile);

#if UNITY_EDITOR
            if (profile == k_DefaultProfile)
            {
                try
                {
                    var cliArgs = Environment.GetCommandLineArgs();
                    var nameArgIndex = -1;
                    var hasEditorModeArg = false;
                    for (var i = 0; i < cliArgs.Length; i++)
                    {
                        if (cliArgs[i] == k_EditorModeArg)
                        {
                            hasEditorModeArg = true;
                        }
                        if (cliArgs[i] == k_NameArg)
                        {
                            nameArgIndex = i;
                        }
                    }
                    if (hasEditorModeArg)
                    {
                        if (nameArgIndex > 0)
                        {
                            profile = cliArgs[nameArgIndex + 1];
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
#endif
            return profile;
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
