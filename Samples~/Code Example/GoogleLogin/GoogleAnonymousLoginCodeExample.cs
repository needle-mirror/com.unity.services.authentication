using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;

namespace Unity.Services.Authentication.Samples
{
    public class GoogleAnonymousLoginCodeExample : MonoBehaviour
    {
        async void Start()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                Debug.LogError("Switch target platform to Android to use this example!");
                return;
            }

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

            //If you’re not using anonymous sign-in and intend to implement user logins via a Google Play Games account,
            //use the SignInWithSessionToken() method. This API doesn't create a new user if the stored login information does not exist.

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        /// <summary>
        /// When the player wants to upgrade from being anonymous to creating a Google Play Games social account and sign in,
        /// your game should prompt the player to trigger the Google Play Games sign-in and get the ID token from Google.
        /// Then, call the following API to link the user to the Google ID token
        /// </summary>
        /// <param name="idToken"></param>
        /// <returns></returns>
        public static async Task LinkWithGoogleAsync(string idToken)
        {
            try
            {
                await AuthenticationService.Instance.LinkWithGoogleAsync(idToken);
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
        /// When the player triggers the Google Play Games sign-in by signing in or by creating a new user profile,
        /// and you have received the Google ID token, call the following API to authenticate the user
        /// </summary>
        /// <param name="idToken"></param>
        /// <returns></returns>
        public static async Task SignInWithGoogleAsync(string idToken)
        {
            //Sign out before Signing In
            AuthenticationService.Instance.SignOut();

            try
            {
                await AuthenticationService.Instance.SignInWithGoogleAsync(idToken);
            }
            catch (RequestFailedException exception)
            {
                Debug.LogError($"Sign in failed with error code: {exception.ErrorCode}");
            }
        }
    }
}
