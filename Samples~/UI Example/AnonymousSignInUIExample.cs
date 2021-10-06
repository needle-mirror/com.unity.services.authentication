using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Authentication.Samples
{
//Anonymous sign-in creates a new user for the game session without any input from the player and is a quick way for a player
//to get started with your game. The following UI sample shows how to set up the ability for players to sign in anonymously
//in your game and get your access token. If a player has already signed in before, the SignInAnonymously() recovers the existing
//login of a user whether they signed in anonymously or through a social account.
    public class AnonymousSignInUIExample : MonoBehaviour
    {
        [SerializeField]
        Button signInButton;
        [SerializeField]
        Text signInButtonText;
        [SerializeField]
        Button signOutButton;
        [SerializeField]
        Text signOutButtonText;

        async void Start()
        {
            //UnityServices.Initialize() will initialize all services that are subscribed to Core
            await UnityServices.InitializeAsync();
            Debug.Log($"Unity services initialization: {UnityServices.State}");

            AuthenticationService.Instance.SignedIn += () =>
            {
                //Shows how to get a playerID
                Debug.Log($"PlayedID: {AuthenticationService.Instance.PlayerId}");

                //Shows how to get an access token
                Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

                //Enable Sign Out button
                signOutButton.interactable = true;

                //Reset sign out button text
                signOutButtonText.text = "Sign Out";

                //Disable Sign In button
                signInButton.interactable = false;

                const string successMessage = "Sign in anonymously succeeded!";
                Debug.Log(successMessage);
                signInButtonText.text = successMessage;
            };

            AuthenticationService.Instance.SignedOut += () =>
            {
                signOutButtonText.text = "Signed Out";

                //Disable Sign out button
                signOutButton.interactable = false;

                //Reset sign in button text
                signInButtonText.text = "Sign In Anonymously";

                //Enable Sign in button
                signInButton.interactable = true;

                Debug.Log("Signed Out!");
            };
            //You can listen to events to display custom messages
            AuthenticationService.Instance.SignInFailed += errorResponse =>
            {
                Debug.LogError($"Sign in anonymously failed with error code: {errorResponse.ErrorCode}");
            };
        }

        public async void OnClickSignIn()
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (RequestFailedException exception)
            {
                Debug.LogError($"Sign in anonymously failed with error code: {exception.ErrorCode}");
            }
        }

        public void OnClickSignOut() => AuthenticationService.Instance.SignOut();
    }
}
