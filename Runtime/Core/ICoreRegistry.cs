using Unity.Services.Core.Internal;

namespace Unity.Services.Authentication
{
    interface ICoreRegistry
    {
        T GetServiceComponent<T>() where T : IServiceComponent;
        void RegisterServiceComponent<T>(T component) where T : IServiceComponent;
        void RegisterService<T>(T service);
    }

    class CoreRegistryAdapter : ICoreRegistry
    {
        readonly CoreRegistry m_Registry;

        public CoreRegistryAdapter(CoreRegistry registry)
        {
            m_Registry = registry;
        }

        public T GetServiceComponent<T>() where T : IServiceComponent
            => m_Registry.GetServiceComponent<T>();

        public void RegisterServiceComponent<T>(T component) where T : IServiceComponent
            => m_Registry.RegisterServiceComponent(component);

        public void RegisterService<T>(T service)
            => m_Registry.RegisterService(service);
    }
}
