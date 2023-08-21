using UnityEngine;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// Contains information on the Code-Link sign in code provided
    /// </summary>
    public sealed class SignInCodeInfo
    {
        /// <summary>
        /// Sign In Code for Code-Link
        /// </summary>
        public string SignInCode { get; internal set; }

        /// <summary>
        /// Date the code will expire
        /// </summary>
        public string Expiration { get; internal set; }

        /// <summary>
        /// Optional field to identify the device that requested the Code-Link
        /// </summary>
        public string Identifier { get; internal set; }
    }
}
