using Unity.Services.Core;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// The entry class to the Authentication Service.
    /// </summary>
    public static class AuthenticationService
    {
        static IAuthenticationService s_Instance;

        /// <summary>
        /// The default singleton instance to access the Authentication service.
        /// </summary>
        /// <exception cref="ServicesInitializationException">
        /// This exception is thrown if the <code>UnityServices.InitializeAsync()</code>
        /// has not finished before accessing the singleton.
        /// </exception>
        public static IAuthenticationService Instance
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
