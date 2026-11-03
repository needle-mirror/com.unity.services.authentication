namespace Unity.Services.Authentication.Cli
{
    /// <summary>
    /// The constants this package's CLI commands use: their groups, tags, and the other literals they depend on,
    /// so each has one source of truth. Keep one class per package: a const read from another assembly is
    /// compiled into the reader, so a shared class would go stale in every package not rebuilt with it.
    /// </summary>
    static class AuthenticationCliConstants
    {
        public const string Group = "services.authentication";
        /// <summary>The Unity Player Accounts subgroup of <see cref="Group"/>.</summary>
        public const string PlayerAccountsGroup = Group + ".player_accounts";
        /// <summary>com.unity.services.core's command group. Must match <c>CoreCliConstants.Group</c>.</summary>
        public const string CoreGroup = "services.core";
        /// <summary>com.unity.pipeline's Play Mode command group, which failure messages point the caller at.</summary>
        public const string PlayModeGroup = "editor.playmode";
        /// <summary>com.unity.pipeline's scene command group, which failure messages point the caller at.</summary>
        public const string SceneGroup = "scene";
        /// <summary>com.unity.pipeline's component command group, which failure messages point the caller at.</summary>
        public const string ComponentGroup = "component";
        /// <summary>Marks commands that target a running Player (require Play Mode with Unity Services initialized).</summary>
        public const string Runtime = "runtime";
        /// <summary>
        /// The GameObject the scene component commands host their component on when none is named. Must match
        /// <c>CoreCliConstants.DefaultGameObjectName</c>, so both components land on one GameObject.
        /// </summary>
        public const string DefaultGameObjectName = "Unity Services";
        public const string PlayerAuthenticationUndoName = "Create Player Authentication";
        /// <summary>Profile names <c>IAuthenticationService.SwitchProfile</c> accepts; it throws on anything else.</summary>
        public const string ProfilePattern = @"^[a-zA-Z0-9_-]{1,30}$";
        /// <summary>The platforms a Player Accounts client can sign players in on.</summary>
        public static readonly string[] PlayerAccountsPlatforms = { "mobile", "desktop" };
        public const string PlayerAccountsPlatformList = "mobile, desktop";
        /// <summary>The social providers a game can register its own app for with Player Accounts.</summary>
        public static readonly string[] PlayerAccountsIdentityProviders = { "apple", "google", "facebook" };
        public const string PlayerAccountsIdentityProviderList = "apple, google, facebook";
        // Limits the Player Accounts service validates; the commands check them first so the caller learns which option to fix.
        public const int PlayerAccountsMinNameLength = 3;
        public const int PlayerAccountsMaxNameLength = 128;
        public const int PlayerAccountsMaxLinkLength = 512;
        public const int PlayerAccountsMaxProviderClientIdLength = 256;
        /// <summary>How long enabling Player Accounts waits for the accepted client to be ready.</summary>
        public const int PlayerAccountsSetupTimeoutSeconds = 30;
        public const int PlayerAccountsSetupPollIntervalSeconds = 2;
        /// <summary>Where the Editor links the project to a Unity Cloud project, which failure messages point the caller at.</summary>
        public const string ServicesProjectSettings = "Edit > Project Settings > Services";
        /// <summary>The Unity Dashboard page where members and their roles are managed.</summary>
        public const string MembersDashboardPath = "Administration > Members in the Unity Dashboard";
        /// <summary>The role that grants the player authentication admin APIs, which Player Accounts setup needs.</summary>
        public const string AuthenticationEditorRole = "Authentication Editor";
        /// <summary>The role that grants read access to player authentication resources.</summary>
        public const string AuthenticationViewerRole = "Authentication Viewer";
        /// <summary>The Dashboard's group for the Authentication roles in a member's role editor.</summary>
        public const string AuthenticationRolesCategory = "LiveOps";
    }
}
