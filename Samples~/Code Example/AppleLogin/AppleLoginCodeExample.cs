using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;

namespace Unity.Services.Authentication.Samples
{
    public class AppleLoginCodeExample : MonoBehaviour
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

            //If you’re not using anonymous sign-in and intend to implement user logins via an Apple account,
            //use the SignInWithSessionToken() method. This API doesn't create a new user if the stored login information does not exist
            await AuthenticationService.Instance.SignInWithSessionTokenAsync();
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
