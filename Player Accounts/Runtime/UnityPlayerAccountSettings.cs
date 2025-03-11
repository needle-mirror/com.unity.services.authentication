using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts
{
    /// <summary>
    /// Unity Player Accounts Settings
    /// </summary>
    class UnityPlayerAccountSettings : ScriptableObject
    {
        const string k_DeepLinkUriScheme = "unitydl";
        const string k_DeepLinkUriHostPrefix = "com.unityplayeraccounts.";

        /// <summary>
        /// Unity Player Accounts Client ID.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        [Tooltip("Unity Player Account Client ID.")]
        internal string clientId;

        /// <summary>
        /// Scope mask, defaults to all scopes selected
        /// </summary>
        [HideInInspector]
        [SerializeField]
        internal int scopeMask = (1 << Enum.GetNames(typeof(SupportedScopesEnum)).Length) - 1;

        /// <summary>
        /// useCustomDeepLinkUri to override deep link uri'
        /// </summary>
        [HideInInspector]
        [SerializeField]
        [Tooltip("Override the default redirect uri")]
        internal bool useCustomDeepLinkUri;

        /// <summary>
        /// custom scheme.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        [Tooltip("Custom Deep Link URI Scheme")]
        internal string customScheme;

        /// <summary>
        /// Custom host.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        [Tooltip("Custom Deep Link URI Host Prefix")]
        internal string customHost;

        /// <summary>
        /// Supported scopes dictionary mapping enums to strings
        /// </summary>
        static readonly Dictionary<SupportedScopesEnum, string> k_SupportedScopesDictionary = new Dictionary<SupportedScopesEnum, string>
        {
            { SupportedScopesEnum.OpenId, "openid" },
            { SupportedScopesEnum.Email, "email" },
            { SupportedScopesEnum.OfflineAccess, "offline_access" }
        };

        /// <summary>
        /// Supported scope enums. 'All' and 'Empty' options grant all available scopes.
        /// </summary>
        [Flags]
        public enum SupportedScopesEnum
        {
            /// <summary>
            /// The OpenID scope. It provides authentication-related scopes, typically used for single sign-on.
            /// </summary>
            OpenId = 1 << 0,

            /// <summary>
            /// The Email scope. This scope is used when the application needs to access the user's email.
            /// </summary>
            Email = 1 << 1,

            /// <summary>
            /// The OfflineAccess scope. This scope is used to get a refresh token that can be used to maintain access to resources when the user is not logged in.
            /// </summary>
            OfflineAccess = 1 << 2,
        }

        /// <summary>
        /// Scope Flags
        /// </summary>
        public SupportedScopesEnum ScopeFlags
        {
            get => (SupportedScopesEnum)scopeMask;
            set => scopeMask = (int)value;
        }

        /// <summary>
        /// Unity Player Accounts Client ID.
        /// </summary>
        public string ClientId
        {
            get
            {
                var trimmedClientId = clientId?.Trim();
                return string.IsNullOrEmpty(trimmedClientId) ? null : trimmedClientId;
            }
            set => clientId = value.Trim();
        }

        /// <summary>
        /// The scope of access that your player account requires. Example: 'openid;email'
        /// </summary>
        public string Scope
        {
            get
            {
                var scope = "";
                var scopeFlags = ScopeFlags;

                foreach (var kvp in k_SupportedScopesDictionary)
                {
                    if (scopeFlags.HasFlag(kvp.Key))
                    {
                        scope += kvp.Value + ";";
                    }
                }

                return scope.TrimEnd(';');
            }
        }

        /// <summary>
        /// Returns true if using a custom uri
        /// </summary>
        public bool UseCustomUri => useCustomDeepLinkUri;

        /// <summary>
        /// Scheme for the deep link Uri for Android and iOS platforms.
        /// </summary>
        public string DeepLinkUriScheme => useCustomDeepLinkUri ? customScheme : k_DeepLinkUriScheme;

        /// <summary>
        /// Prefix value for the deep link Uri Host name for Android and iOS platforms.
        /// </summary>
        public string DeepLinkUriHostPrefix => useCustomDeepLinkUri ? customHost : k_DeepLinkUriHostPrefix;

        /// <summary>
        /// The instance of the PlayerAccountSettings class.
        /// </summary>
        public static UnityPlayerAccountSettings Load()
        {
            return Resources.Load<UnityPlayerAccountSettings>(nameof(UnityPlayerAccountSettings));
        }
    }
}
