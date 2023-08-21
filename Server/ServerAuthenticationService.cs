using Unity.Services.Core;

namespace Unity.Services.Authentication.Server
{
    /// <summary>
    /// The entry class to the Authentication Service.
    /// </summary>
    public static class ServerAuthenticationService
    {
        static IServerAuthenticationService s_Instance;

        /// <summary>
        /// Gets the instance of the server authentication service.
        /// </summary>
        /// <exception cref="ServicesInitializationException">
        /// Thrown when the singleton is not initialized.
        /// Call UnityServices.InitializeAsync() to initialize.
        /// </exception>
        public static IServerAuthenticationService Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    throw new ServicesInitializationException("Singleton is not initialized. " +
                        "Please call UnityServices.InitializeAsync() to initialize.");
                }

                return s_Instance;
            }
            internal set => s_Instance = value;
        }
    }
}
