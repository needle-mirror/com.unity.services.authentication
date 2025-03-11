using System.Threading.Tasks;
using Unity.Services.Authentication.Server.Environments.Generated;
using Unity.Services.Authentication.Server.Internal;
using Unity.Services.Authentication.Server.Proxy.Generated;
using Unity.Services.Authentication.Server.ServiceAuth.Generated;
using Unity.Services.Authentication.Server.Shared;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Environments.Internal;
using Unity.Services.Core.Internal;
using Unity.Services.Core.Scheduler.Internal;
using UnityEngine;

namespace Unity.Services.Authentication.Server
{
    class ServerAuthenticationInitializer : IInitializablePackageV2
    {
        const string k_CloudEnvironmentKey = "com.unity.services.core.cloud-environment";
        const string k_StagingEnvironment = "staging";
        const string k_GatewayStagingPath = "https://staging.services.api.unity.com";
        const string k_GatewayProductionPath = "https://services.api.unity.com";
        const string k_ProxyPath = "http://localhost:8086";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitializeOnLoad()
        {
            var initializer = new ServerAuthenticationInitializer();
            initializer.Register(CorePackageRegistry.Instance);
        }

        public void Register(CorePackageRegistry registry)
        {
            registry.Register(this)
                .DependsOn<ICloudProjectId>()
                .DependsOn<IEnvironments>()
                .DependsOn<IProjectConfiguration>()
                .ProvidesComponent<IServerEnvironmentId>()
                .ProvidesComponent<IServerAccessToken>();
        }

        public Task Initialize(CoreRegistry registry)
        {
            ServerAuthenticationService.Instance = InitializeService(registry);
            return Task.CompletedTask;
        }

        public Task InitializeInstanceAsync(CoreRegistry registry)
        {
            InitializeService(registry);
            return Task.CompletedTask;
        }

        ServerAuthenticationServiceInternal InitializeService(CoreRegistry registry)
        {
            var settings = new ServerAuthenticationSettings();
            var serverAccessToken = new ServerAccessTokenComponent();
            var serverEnvironmentId = new ServerEnvironmentIdComponent();
            var scheduler = registry.GetServiceComponent<IActionScheduler>();
            var dateTime = new DateTimeWrapper();
            var jwtDecoder = new JwtDecoder(dateTime);
            var cloudProjectId = registry.GetServiceComponent<ICloudProjectId>();
            var environment = registry.GetServiceComponent<IEnvironments>();
            var projectConfig = registry.GetServiceComponent<IProjectConfiguration>();

            var gatewayConfiguration = new ApiConfiguration();
            gatewayConfiguration.BasePath = GetHost(projectConfig);
            var proxyConfiguration = new ApiConfiguration();
            proxyConfiguration.BasePath = k_ProxyPath;
            var serverConfiguration = new ServerConfiguration(gatewayConfiguration, proxyConfiguration);
            var apiClient = new AuthenticationServerApiClient(serverConfiguration);

            var environmentApi = new EnvironmentApi(apiClient, gatewayConfiguration);
            var authApi = new ServiceAuthenticationApi(apiClient, gatewayConfiguration);
            var proxyApi = new ProxyApi(apiClient, proxyConfiguration);

            var authenticationService = new ServerAuthenticationServiceInternal(
                settings,
                cloudProjectId,
                jwtDecoder,
                scheduler,
                dateTime,
                environment,
                serverAccessToken,
                serverEnvironmentId,
                serverConfiguration,
                environmentApi,
                authApi,
                proxyApi
            );

            registry.RegisterService<IServerAuthenticationService>(authenticationService);
            registry.RegisterServiceComponent<IServerAccessToken>(serverAccessToken);
            registry.RegisterServiceComponent<IServerEnvironmentId>(serverEnvironmentId);

            return authenticationService;
        }

        static string GetHost(IProjectConfiguration projectConfiguration)
        {
            var cloudEnvironment = projectConfiguration?.GetString(k_CloudEnvironmentKey);

            switch (cloudEnvironment)
            {
                case k_StagingEnvironment:
                    return k_GatewayStagingPath;
                default:
                    return k_GatewayProductionPath;
            }
        }
    }
}
