using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;

namespace Unity.Services.Authentication.Samples
{
    public class SteamLoginCodeExample : MonoBehaviour
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
            AuthenticationService.Instance.SignInFailed += (errorResponse) =>
            {
                Debug.LogError($"Sign In Failed with error code: {errorResponse.ErrorCode}");
            };

            //If you’re not using anonymous login in your game and intend to implement user logins using a Steam account, use the SignInWithSessionToken() method.
            //This API doesn't create a new user if the stored login information does not exist
            await AuthenticationService.Instance.SignInWithSessionTokenAsync();
        }

        /// <summary>
        /// When the player triggers the Steam login to sign in or create a new user,
        /// and you have received the Steam session ticket, call the following API to sign-in the user
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
