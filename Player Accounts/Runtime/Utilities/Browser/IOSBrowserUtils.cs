#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using Unity.Services.Core.Configuration.Internal;

namespace Unity.Services.Authentication.PlayerAccounts
{
    /// <summary>
    /// Provides a set of utility functions for interacting with a web browser on iOS.
    /// </summary>
    class IOSBrowserUtils : IBrowserUtils
    {
        /// <summary>
        /// Native -> managed callback delivering the OAuth redirect (or an error) from the auth session.
        /// </summary>
        delegate void AuthSessionCallback(string callbackUrl, string error);

        [DllImport("__Internal")]
        static extern void launchUnityPlayerAccountUrl(string url, string callbackScheme);
        [DllImport("__Internal")]
        static extern void setUnityPlayerAccountAuthCallback(AuthSessionCallback callback);

        // The active instance the static (AOT) trampoline routes the native callback to.
        static IOSBrowserUtils s_Active;

        readonly ICloudProjectId m_CloudProjectId;
        readonly UnityPlayerAccountSettings m_Settings;
        readonly PlayerAccountServiceInternal m_AccountService;

        public IOSBrowserUtils(ICloudProjectId cloudProjectId, UnityPlayerAccountSettings settings, PlayerAccountServiceInternal accountService)
        {
            m_CloudProjectId = cloudProjectId;
            m_Settings = settings;
            m_AccountService = accountService;
        }

        public Task LaunchUrlAsync(string url)
        {
            s_Active = this;
            setUnityPlayerAccountAuthCallback(OnAuthSessionCallback);
            // Pass the deep-link scheme (e.g. "unitydl") so ASWebAuthenticationSession can
            // intercept the redirect to it and return the callback URL directly.
            launchUnityPlayerAccountUrl(url, m_Settings.DeepLinkUriScheme);
            return Task.CompletedTask;
        }

        [MonoPInvokeCallback(typeof(AuthSessionCallback))]
        static void OnAuthSessionCallback(string callbackUrl, string error)
        {
            // This method is invoked directly from native code as a reverse P/Invoke callback.
            // Any managed exception that escapes here would cross the native boundary and abort
            // the IL2CPP runtime (hard crash). Downstream handling (OnDeepLinkActivated) can throw
            // on a malformed callback URL or when the redirect carries an OAuth error, so we must
            // ensure nothing propagates past this point.
            try
            {
                s_Active?.OnWebAuthSessionCompleted(callbackUrl, error);
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
        }

        public bool Bind()
        {
            return true;
        }

        private void OnWebAuthSessionCompleted(string callbackUrl, string error)
        {
            if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(callbackUrl))
            {
                Logger.Log($"Player Accounts web authentication session ended without a callback. {error}");

                if (m_AccountService.SignInState == PlayerAccountState.SigningIn)
                {
                    m_AccountService.SignOut();
                }

                return;
            }

            m_AccountService.OnDeepLinkActivated(callbackUrl);
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
