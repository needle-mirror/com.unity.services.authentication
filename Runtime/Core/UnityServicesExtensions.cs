using Unity.Services.Authentication;

namespace Unity.Services.Core
{
    /// <summary>
    /// Authentication extension methods
    /// </summary>
    public static class UnityServicesExtensions
    {
        /// <summary>
        /// Retrieve the authentication service from the core service registry
        /// </summary>
        /// <param name="unityServices">The core services instance</param>
        /// <returns>The authentication service instance</returns>
        public static IAuthenticationService GetAuthenticationService(this IUnityServices unityServices)
        {
            return unityServices.GetService<IAuthenticationService>();
        }
    }
}
