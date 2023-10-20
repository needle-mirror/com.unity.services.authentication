using Unity.Services.Authentication.Server;

namespace Unity.Services.Core
{
    /// <summary>
    /// Authentication extension methods
    /// </summary>
    public static class UnityServicesExtensions
    {
        /// <summary>
        /// Retrieve the server authentication service from the core service registry
        /// </summary>
        /// <param name="unityServices">The core services instance</param>
        /// <returns>The server authentication service instance</returns>
        public static IServerAuthenticationService GetServerAuthenticationService(this IUnityServices unityServices)
        {
            return unityServices.GetService<IServerAuthenticationService>();
        }
    }
}
