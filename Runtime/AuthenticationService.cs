using System;
using UnityEngine;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// The entry class to the Authentication Service.
    /// </summary>
    public static class AuthenticationService
    {
        /// <summary>
        /// The default singleton instance to access the Authentication service.
        /// </summary>
        public static IAuthenticationService Instance { get; internal set; }
    }
}
