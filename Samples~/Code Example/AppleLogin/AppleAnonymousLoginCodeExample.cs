using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;

namespace Unity.Services.Authentication.Samples
{
    public class AppleAnonymousLoginCodeExample : MonoBehaviour
    {
        async void Start()
        {
            if (Application.platform != RuntimePlatform.IPhonePlayer)
            {
                Debug.LogError("Switch target platform to IOS to use this example!");
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

            //If you’re using anonymous sign-in, call the SignInAnonymously() method when the user is prompted to sign in.
            //This method checks if the user has already signed in before and re-authenticates the user.
            //If there is no user sign-in information, this method creates a new anonymous user
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        /// <summary>
        /// When the player wants to upgrade from being anonymous to creating an Apple social account and sign in using Apple,
        /// your game should prompt the player to trigger the Apple sign-in and get the ID token from Apple.
        /// Then, call the following API to link the user to the Apple ID token
        /// </summary>
        /// <param name="idToken"></param>
        /// <returns></returns>
        public static async Task LinkWithAppleAsync(string idToken)
        {
            try
            {
                await AuthenticationService.Instance.LinkWithAppleAsync(idToken);
                Debug.Log("Link is successful.");
            }
            catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
            {
                //Prompt the player with an error message.
                Debug.LogError("This user is already linked with another account. Log in instead.");
            }
            catch (Exception ex)
            {
                Debug.LogError("Link failed.");
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// When the player triggers the Apple sign-in by signing in or by creating a new user profile,
        /// and you have received the Apple ID token, call the following API to authenticate the user
        /// </summary>
        /// <param name="idToken"></param>
        /// <returns></returns>
        public static async Task SignInWithAppleAsync(string idToken)
        {
            //Sign out before Signing In
            AuthenticationService.Instance.SignOut();

            try
            {
                await AuthenticationService.Instance.SignInWithAppleAsync(idToken);
            }
            catch (RequestFailedException exception)
            {
                Debug.LogError($"Sign in failed with error code: {exception.ErrorCode}");
            }
        }
    }
}
