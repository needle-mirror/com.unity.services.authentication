#if UNITY_PIPELINE_AVAILABLE
using System;
using Unity.Pipeline.Commands;
using Unity.Services.Core;
using UnityEngine;

namespace Unity.Services.Authentication.Cli
{
    static class AuthenticationRuntimeCliCommands
    {
        [CliGroupCommand(AuthenticationCliConstants.Group, "Get the active authentication profile scope. Requires to be at runtime with Unity Services initialized", Tags = new[] { AuthenticationCliConstants.Runtime })]
        public static string GetProfile()
        {
            return InitializedService().Profile ?? string.Empty;
        }

        [CliGroupCommand(AuthenticationCliConstants.Group, "Get the authentication state (SignedOut, SigningIn, Authorized, Refreshing, Expired). Requires to be at runtime with Unity Services initialized", Tags = new[] { AuthenticationCliConstants.Runtime })]
        public static string GetState()
        {
            return InitializedService().State.ToString();
        }

        [CliGroupCommand(AuthenticationCliConstants.Group, "Return whether the player is authorized, i.e. holds a still-valid access token. Requires to be at runtime with Unity Services initialized", Tags = new[] { AuthenticationCliConstants.Runtime })]
        public static bool GetAuthorized()
        {
            return InitializedService().IsAuthorized;
        }

        [CliGroupCommand(AuthenticationCliConstants.Group, "Return whether the player is signed in. Requires to be at runtime with Unity Services initialized", Tags = new[] { AuthenticationCliConstants.Runtime })]
        public static bool GetSignedIn()
        {
            return InitializedService().IsSignedIn;
        }

        [CliGroupCommand(AuthenticationCliConstants.Group, "Get the signed-in player's id. Requires to be at runtime with Unity Services initialized", Tags = new[] { AuthenticationCliConstants.Runtime })]
        public static string GetPlayerId()
        {
            return InitializedService().PlayerId ?? string.Empty;
        }

        [CliGroupCommand(AuthenticationCliConstants.Group, "Get the signed-in player's raw access token (JWT). Requires to be at runtime with Unity Services initialized and the player signed in", Tags = new[] { AuthenticationCliConstants.Runtime })]
        public static string GetAccessToken()
        {
            return InitializedService().AccessToken ?? string.Empty;
        }

        [CliGroupCommand(AuthenticationCliConstants.Group, "Get the current player name. Requires to be at runtime with Unity Services initialized and the player signed in and fetched the player name", Tags = new[] { AuthenticationCliConstants.Runtime })]
        public static string GetPlayerName()
        {
            return InitializedService().PlayerName ?? string.Empty;
        }

        [CliGroupCommand(AuthenticationCliConstants.Group, "Get the signed-in player's info (id, created-at, linked identities, username, last password update). Requires to be at runtime with Unity Services initialized, the player signed in and fetched the player info", Tags = new[] { AuthenticationCliConstants.Runtime })]
        public static PlayerInfo GetPlayerInfo()
        {
            return InitializedService().PlayerInfo;
        }

        static IAuthenticationService InitializedService()
        {
            if (!Application.isPlaying)
            {
                throw new InvalidOperationException(
                    "The authentication service only exists at runtime, and the Editor is not in Play Mode. "
                    + "Enter Play Mode with " + AuthenticationCliConstants.PlayModeGroup + " start, wait until " + AuthenticationCliConstants.CoreGroup + " get_initialization_state returns Initialized, "
                    + "then run this command again.");
            }

            var state = UnityServices.State;
            if (state != ServicesInitializationState.Initialized)
            {
                throw new InvalidOperationException(
                    $"Unity Services are {state}, so the authentication service is not available yet. "
                    + "Wait until " + AuthenticationCliConstants.CoreGroup + " get_initialization_state returns Initialized, then run this command again. "
                    + "If it stays Uninitialized, nothing initializes the services: exit Play Mode and run "
                    + AuthenticationCliConstants.CoreGroup + " add_services_initialization_component, or call UnityServices.InitializeAsync from your own code. "
                    + "If it reports Failed, the Console holds the initialization error.");
            }

            return AuthenticationService.Instance;
        }
    }
}
#endif
