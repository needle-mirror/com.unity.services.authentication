using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;

namespace Unity.Services.Authentication.Samples
{
    public class SteamAnonymousLoginCodeExample : MonoBehaviour
    {
        async void Start()
        {
            await UnityServices.InitializeAsync();
            Debug.Log($"Unity services initialization :{UnityServices.State}");

            AuthenticationService.Instance.SignedIn += () =>
            {
                //Shows how to get a playerID
                Debug.Log($"PlayedID: {AuthenticationService.Instance.PlayerId}");

                //Shows how to get an access token
                Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");
            };
            //You can listen to events to display custom messages
            AuthenticationService.Instance.SignInFailed += errorResponse =>
            {
                Debug.LogError($"Sign In Failed with error code: {errorResponse.ErrorCode}");
            };

            //If you’re using anonymous login in your game, call the SignInAnonymously() method when the user is prompted to sign in.
            //This method checks whether the user has already signed in before and re-authenticates the user.
            //If there is no user login information, this method creates a new anonymous user
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        /// <summary>
        /// When the player wants to upgrade from being anonymous to creating a Steam account and sign in using a Steam account,
        /// the game should prompt the player to trigger the Steam login and get the session ticket from Steam.
        /// Then, call the following API to link the user to the Steam session ticket
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public static async Task LinkWithSteamAsync(string ticket)
        {
            try
            {
                await AuthenticationService.Instance.LinkWithSteamAsync(ticket);
                Debug.Log("Link is successful.");
            }
            catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
            {
                // Prompt the player with an error message.
                Debug.LogError("This user is already linked with another account. Log in instead.");
            }
            catch (Exception ex)
            {
                Debug.LogError("Link failed.");
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// When the player triggers the Steam login by signing in or by creating a new user profile,
        /// and you have received the Steam session ticket, call the following API to authenticate the user
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public static async Task SignInWithSteamAsync(string ticket)
        {
            //Sign out before Signing In
            AuthenticationService.Instance.SignOut();

            try
            {
                await AuthenticationService.Instance.SignInWithSteamAsync(ticket);
            }
            catch (RequestFailedException exception)
            {
                Debug.LogError($"Sign in failed with error code: {exception.ErrorCode}");
            }
        }
    }
}
