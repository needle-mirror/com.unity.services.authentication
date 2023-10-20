#if UNITY_ANDROID
using System.Threading.Tasks;
using Unity.Services.Core.Configuration.Internal;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts
{
    /// <summary>
    /// Provides a set of utility functions for interacting with a web browser on Android.
    /// </summary>
    class AndroidBrowserUtils : IBrowserUtils
    {
        readonly ICloudProjectId m_CloudProjectId;
        readonly UnityPlayerAccountSettings m_Settings;

        public AndroidBrowserUtils(ICloudProjectId cloudProjectId, UnityPlayerAccountSettings settings)
        {
            m_CloudProjectId = cloudProjectId;
            m_Settings = settings;
        }

        public Task LaunchUrlAsync(string url)
        {
            Application.OpenURL(url);

            return Task.CompletedTask;
        }

        public bool Bind()
        {
            return true;
        }

        public void Dismiss()
        {
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
