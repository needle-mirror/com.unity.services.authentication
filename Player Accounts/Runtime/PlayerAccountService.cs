using Unity.Services.Core;

namespace Unity.Services.Authentication.PlayerAccounts
{
    /// <summary>
    /// The entry class to the Player Account Service.
    /// </summary>
    public static class PlayerAccountService
    {
        static IPlayerAccountService s_Instance;

        /// <summary>
        /// The default singleton instance to access the Player Account service.
        /// </summary>
        /// <exception cref="ServicesInitializationException">
        /// This exception is thrown if the <c>UnityServices.InitializeAsync()</c>
        /// has not finished before accessing the singleton.
        /// </exception>
        public static IPlayerAccountService Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    throw new ServicesInitializationException("Singleton is not initialized. " +
                        "Please call UnityServices.InitializeAsync() to initialize. " +
                        "Please make sure Player Accounts is configured in the Unity Editor Settings");
                }

                return s_Instance;
            }
            internal set => s_Instance = value;
        }
    }
}
