using System.Threading.Tasks;
using Unity.Services.Authentication.Internal;
using Unity.Services.Authentication.Utilities;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Environments.Internal;
using Unity.Services.Core.Internal;
using Unity.Services.Core.Scheduler.Internal;
using UnityEngine;

namespace Unity.Services.Authentication
{
    class AuthenticationPackageInitializer : IInitializablePackage
    {
#if AUTHENTICATION_TESTING_STAGING_UAS
        const string k_UasHost = "https://api.stg.identity.corp.unity3d.com";
#else
        const string k_UasHost = "https://api.prd.identity.corp.unity3d.com";
#endif

        public Task Initialize(CoreRegistry registry)
        {
            var settings = new AuthenticationSettings();
            var scheduler = registry.GetServiceComponent<IActionScheduler>();
            var projectId = registry.GetServiceComponent<ICloudProjectId>();
            var projectConfiguration = registry.GetServiceComponent<IProjectConfiguration>();
            var profile = new Profile(projectConfiguration.GetString(ProfileOptionsExtensions.ProfileKey, "default"));
            var dateTime = new DateTimeWrapper();
            var networkUtilities = new NetworkingUtilities(scheduler);
            var networkClient = new AuthenticationNetworkClient(k_UasHost,
                projectId.GetCloudProjectId(),
                registry.GetServiceComponent<IEnvironments>(),
                new CodeChallengeGenerator(),
                networkUtilities);
            var authenticationService = new AuthenticationServiceInternal(
                settings,
                networkClient,
                profile,
                new JwtDecoder(dateTime),
                new AuthenticationCache(projectId, profile),
                scheduler,
                dateTime);

            AuthenticationService.Instance = authenticationService;
            registry.RegisterServiceComponent<IPlayerId>(new PlayerIdComponent(authenticationService));
            registry.RegisterServiceComponent<IAccessToken>(new AccessTokenComponent(authenticationService));

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
                .ProvidesComponent<IPlayerId>()
                .ProvidesComponent<IAccessToken>();
        }
    }
}
