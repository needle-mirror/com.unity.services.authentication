using Unity.Services.Authentication.PlayerAccounts;

namespace Unity.Services.Core
{
    public static class UnityServicesExtensions
    {
        /// <summary>
        /// Retrieve the player account service from the core service registry
        /// </summary>
        /// <param name="unityServices">The core services instance</param>
        /// <returns>The player account service instance</returns>
        public static IPlayerAccountService GetPlayerAccountService(this IUnityServices unityServices)
        {
            return unityServices.GetService<IPlayerAccountService>();
        }
    }
}
