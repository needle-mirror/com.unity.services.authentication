using System.Threading.Tasks;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Internal;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts
{
    class PlayerAccountsPackageInitializer : IInitializablePackageV2
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitializeOnLoad()
        {
            var initializer = new PlayerAccountsPackageInitializer();
            initializer.Register(CorePackageRegistry.Instance);
        }

        public void Register(CorePackageRegistry registry)
        {
            registry.Register(this)
                .DependsOn<ICloudProjectId>();
        }

        public Task Initialize(CoreRegistry registry)
            => Initialize(new CoreRegistryAdapter(registry));

        public Task InitializeInstanceAsync(CoreRegistry registry)
            => InitializeInstanceAsync(new CoreRegistryAdapter(registry));

        internal Task Initialize(ICoreRegistry registry)
        {
            PlayerAccountService.Instance = InitializeService(registry);
            return Task.CompletedTask;
        }

        internal Task InitializeInstanceAsync(ICoreRegistry registry)
        {
            InitializeService(registry);
            return Task.CompletedTask;
        }

        PlayerAccountServiceInternal InitializeService(ICoreRegistry registry)
        {
            var settings = UnityPlayerAccountSettings.Load();
            if (settings is null) return null;

            var network = new NetworkHandler();
            var dateTime = new DateTimeWrapper();
            var jwtDecoder = new JwtDecoder(dateTime);
            var projectId = registry.GetServiceComponent<ICloudProjectId>();
            var service = new PlayerAccountServiceInternal(settings, projectId, jwtDecoder, network);
            registry.RegisterService<IPlayerAccountService>(service);
            return service;
        }
    }
}
