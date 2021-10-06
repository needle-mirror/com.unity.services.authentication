using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Authentication.Samples
{
    public class FacebookLoginUIExample : MonoBehaviour
    {
        [SerializeField]
        Button linkWithFbButton;
        [SerializeField]
        Button signInWithFbButton;
        [SerializeField]
        Text signInWithFbText;
        [SerializeField]
        InputField tokenInputField;

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
                Debug.LogError($"Sign in anonymously failed with error code: {errorResponse.ErrorCode}");
            };

            //If you’re using anonymous sign in, call the SignInAnonymously() method when the user is prompted to sign in.
            //This method checks whether the user has already signed in before and re-authenticates the user.
            //If there is no user login information, this method creates a new anonymous user
            await AuthenticationService.Instance.SignInAnonymouslyAsync();


            signInWithFbButton.interactable = true;
            linkWithFbButton.interactable = true;
        }

        /// <summary>
        /// When the player wants to upgrade from being anonymous to creating a Facebook social account and sign in using Facebook,
        /// the game should prompt the player to trigger the Facebook login and get the access token from Facebook.
        /// Then, call the following API to link the user to the Facebook Access token
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        static async Task LinkWithFacebookAsync(string accessToken)
        {
            try
            {
                await AuthenticationService.Instance.LinkWithFacebookAsync(accessToken);
                Debug.Log("Link is successful.");
            }
            catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
            {
                Debug.LogError("This user is already linked with another account. Log in instead.");
            }
            catch (Exception ex)
            {
                Debug.LogError("Link failed.");
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// When the player triggers the Facebook login by signing in or by creating a new user profile,
        /// and you have received the Facebook access token, call the following API to authenticate the user
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        async Task SignInWithFacebookAsync(string accessToken)
        {
            //Sign out before attempting to sign in
            AuthenticationService.Instance.SignOut();

            try
            {
                await AuthenticationService.Instance.SignInWithFacebookAsync(accessToken);
                signInWithFbText.text = "Sign In Successful!";
                signInWithFbButton.interactable = false;
            }
            catch (RequestFailedException exception)
            {
                Debug.LogError($"Sign in failed with error code: {exception.ErrorCode}");
            }
        }

        public async void OnClickLinkWithFb() => await LinkWithFacebookAsync(tokenInputField.text);

        public async void OnClickSignInWithFb() => await SignInWithFacebookAsync(tokenInputField.text);
    }
}
