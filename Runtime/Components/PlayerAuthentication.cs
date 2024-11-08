using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Components;
using Unity.Services.Core.Internal;
using UnityEngine;

namespace Unity.Services.Authentication.Components
{
    /// <summary>
    /// Manage authentication in your scenes.
    /// </summary>
    [AddComponentMenu("Services/Player Authentication")]
    public class PlayerAuthentication : ServicesBehaviour
    {
        /// <summary>
        /// Access to the authentication service.
        /// Available after services are successfully initialized.
        /// </summary>
        public IAuthenticationService AuthenticationService { get; internal set; }

        /// <summary>
        /// Option to set a custom profile with <see cref="IAuthenticationService.SwitchProfile(string)"/>.
        /// The profile is a local scope for persisted player credentials that you can use to get different players.
        /// </summary>
        [Header("On Initialization")]
        [SerializeField]
        [Tooltip("Option to set a custom profile to scope persisted credentials and get different players.")]
        public bool SetCustomProfile;

        /// <summary>
        /// The profile value to use if <see cref="SetCustomProfile"/> is true.
        /// The profile is a local scope for persisted player credentials that you can use to get different players.
        /// </summary>
        [SerializeField]
        [Visibility(nameof(SetCustomProfile), true)]
        [Tooltip("The profile is a local scope for persisted player credentials that you can use to get different players.")]
        public string Profile;

        /// <summary>
        /// Sign in anonymously automatically after services initialization.
        /// </summary>
        [SerializeField]
        [Tooltip("Option to sign in anonymously automatically after services initialization.")]
        public bool SignInAnonymously;

        /// <summary>
        /// Fetches the player info upon sign in. This provides the player creation time, username, etc.
        /// </summary>
        [Header("On Sign In")]
        [SerializeField]
        [Tooltip("Fetches the player info upon sign in. This provides the player creation time, username, etc.")]
        public bool FetchPlayerInfo;

        /// <summary>
        /// Fetches the player name upon sign in.
        /// </summary>
        [SerializeField]
        [Tooltip("Fetches the player name upon sign in.")]
        public bool FetchPlayerName;

        /// <summary>
        /// Pass in the option to autogenerate the name if none exist.
        /// Only used if <see cref="FetchPlayerName"/> is true.
        /// </summary>
        [SerializeField]
        [Visibility(nameof(FetchPlayerName), true)]
        [Tooltip("Pass in the option to autogenerate the name if none exist.")]
        public bool GenerateName;

        /// <summary>
        /// Offers <see cref="IAuthenticationService"/> events as unity events.
        /// </summary>
        [Header("Events")]
        [SerializeField]
        public PlayerAuthenticationEvents Events = new PlayerAuthenticationEvents();

        internal bool IsSetupDone;
        internal bool IsInfoFetched;
        internal bool IsNameFetched;

        internal PlayerAuthentication()
        {
        }

        /// <summary>
        /// Called when the services registry is set and ready to be used
        /// </summary>
        protected override void OnServicesReady()
        {
        }

        /// <summary>
        /// Called when the services are initialized and ready to be used
        /// </summary>
        protected override async void OnServicesInitialized()
        {
            if (AuthenticationService == null)
            {
                try
                {
                    SetAuthenticationService();
                    await SetupAsync();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }
        }

        /// <summary>
        /// Called on destroy to cleanup
        /// </summary>
        protected override void Cleanup()
        {
            if (AuthenticationService != null)
            {
                AuthenticationService.SignInFailed -= OnSignInFailed;
                AuthenticationService.SignedOut -= OnSignedOut;
                AuthenticationService.Expired -= OnExpired;
                AuthenticationService.SignInCodeReceived -= OnSignInCodeReceived;
                AuthenticationService.SignInCodeExpired -= OnSignInCodeExpired;
            }
        }

        /// <summary>
        /// Set the authentication service property
        /// </summary>
        internal virtual void SetAuthenticationService()
        {
            AuthenticationService = Services.GetAuthenticationService();
        }

        internal async Task SetupAsync()
        {
            AuthenticationService.SignedIn -= OnSignedIn;
            AuthenticationService.SignedIn += OnSignedIn;
            AuthenticationService.SignInFailed -= OnSignInFailed;
            AuthenticationService.SignInFailed += OnSignInFailed;
            AuthenticationService.SignedOut -= OnSignedOut;
            AuthenticationService.SignedOut += OnSignedOut;
            AuthenticationService.Expired -= OnExpired;
            AuthenticationService.Expired += OnExpired;
            AuthenticationService.SignInCodeReceived -= OnSignInCodeReceived;
            AuthenticationService.SignInCodeReceived += OnSignInCodeReceived;
            AuthenticationService.SignInCodeExpired -= OnSignInCodeExpired;
            AuthenticationService.SignInCodeExpired += OnSignInCodeExpired;

            if (!AuthenticationService.IsSignedIn)
            {
                if (SetCustomProfile)
                {
                    AuthenticationService.SwitchProfile(Profile);
                }

                if (SignInAnonymously)
                {
                    await SignInAnonymouslyAsync();
                }
            }

            IsSetupDone = true;
        }

        async Task SignInAnonymouslyAsync()
        {
            try
            {
                await AuthenticationService.SignInAnonymouslyAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"Authentication Failed!\n{e}");
            }
        }

        async Task FetchPlayerInfoAsync()
        {
            try
            {
                await AuthenticationService.GetPlayerInfoAsync();
                IsInfoFetched = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Fetch Player Info Failed!\n{e}");
            }
        }

        async Task FetchPlayerNameAsync()
        {
            try
            {
                await AuthenticationService.GetPlayerNameAsync(GenerateName);
                IsNameFetched = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Fetch Player Name Failed!\n{e}");
            }
        }

        async void OnSignedIn()
        {
            if (FetchPlayerInfo && !IsInfoFetched)
            {
                await FetchPlayerInfoAsync();
            }

            if (FetchPlayerName && !IsNameFetched)
            {
                await FetchPlayerNameAsync();
            }

            Events?.SignedIn?.Invoke();
        }

        void OnSignInFailed(Exception exception)
        {
            Events?.SignInFailed?.Invoke(exception);
        }

        void OnSignedOut()
        {
            ResetAutomation();
            Events?.SignedOut?.Invoke();
        }

        void OnExpired()
        {
            Events?.Expired?.Invoke();
        }

        void OnSignInCodeReceived(SignInCodeInfo info)
        {
            Events?.SignInCodeReceived?.Invoke(info);
        }

        void OnSignInCodeExpired()
        {
            Events?.SignInCodeExpired?.Invoke();
        }

        void ResetAutomation()
        {
            IsInfoFetched = false;
            IsNameFetched = false;
        }
    }
}
