using System;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.Services.Authentication.Components
{
    /// <summary>
    /// Offers <see cref="IAuthenticationService"/> events as unity events.
    /// </summary>
    [Serializable]
    public class PlayerAuthenticationEvents
    {
        /// <summary>
        /// Offers <see cref="IAuthenticationService.SignedIn"/> as a unity event.
        /// </summary>
        [SerializeField]
        public UnityEvent SignedIn = new UnityEvent();

        /// <summary>
        /// Offers <see cref="IAuthenticationService.SignInFailed"/> as a unity event.
        /// </summary>
        [SerializeField]
        public UnityEvent<Exception> SignInFailed = new UnityEvent<Exception>();

        /// <summary>
        /// Offers <see cref="IAuthenticationService.SignedOut"/> as a unity event.
        /// </summary>
        [SerializeField]
        public UnityEvent SignedOut = new UnityEvent();

        /// <summary>
        /// Offers <see cref="IAuthenticationService.Expired"/> as a unity event.
        /// </summary>
        [SerializeField]
        public UnityEvent Expired = new UnityEvent();

        /// <summary>
        /// Offers <see cref="IAuthenticationService.SignInCodeReceived"/> as a unity event.
        /// </summary>
        [SerializeField]
        public UnityEvent<SignInCodeInfo> SignInCodeReceived = new UnityEvent<SignInCodeInfo>();

        /// <summary>
        /// Offers <see cref="IAuthenticationService.SignInCodeExpired"/> as a unity event.
        /// </summary>
        [SerializeField]
        public UnityEvent SignInCodeExpired = new UnityEvent();
    }
}
