#if UNITY_IOS
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Services.Core.Configuration.Internal;

namespace Unity.Services.Authentication.PlayerAccounts
{
    /// <summary>
    /// Provides a set of utility functions for interacting with a web browser on iOS.
    /// </summary>
    class IOSBrowserUtils : IBrowserUtils
    {
        [DllImport("__Internal")]
        static extern void launchUnityPlayerAccountUrl(string url);
        [DllImport("__Internal")]
        static extern void dismissUnityPlayerAccount();

        readonly ICloudProjectId m_CloudProjectId;
        readonly UnityPlayerAccountSettings m_Settings;

        public IOSBrowserUtils(ICloudProjectId cloudProjectId, UnityPlayerAccountSettings settings)
        {
            m_CloudProjectId = cloudProjectId;
            m_Settings = settings;
        }

        public Task LaunchUrlAsync(string url)
        {
            launchUnityPlayerAccountUrl(url);
            return Task.CompletedTask;
        }

        public bool Bind()
        {
            return true;
        }

        public void Dismiss()
        {
            dismissUnityPlayerAccount();
        }

        public string GetRedirectUri()
        {
            if (m_Settings == null)
            {
                return null;
            }

            return $"{m_Settings.DeepLinkUriScheme}://{(m_Settings.UseCustomUri ? m_Settings.DeepLinkUriHostPrefix : m_Settings.DeepLinkUriHostPrefix + m_CloudProjectId.GetCloudProjectId())}";
        }
    }
}
#endif
