using System;
using Unity.Services.Core;
using UnityEngine;

namespace Unity.Services.Authentication.Samples
{
//Anonymous sign-in creates a new user for the game session without any input from the player and is a quick way for a player
//to get started with your game. The following code sample shows how to set up the ability for players to sign in anonymously
//in your game and get your access token. If a player has already signed in before, the SignInAnonymously() recovers the existing
//login of a user whether they signed in anonymously or through a social account
    public class AnonymousLoginCodeExample : MonoBehaviour
    {
        async void Start()
        {
            //UnityServices.Initialize() will initialize all services that are subscribed to Core
            await UnityServices.InitializeAsync();
            Debug.Log($"Unity services initialization :{UnityServices.State}");

            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                //Shows how to get a playerID
                Debug.Log($"PlayedID: {AuthenticationService.Instance.PlayerId}");

                //Shows how to get an access token
                Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

                Debug.Log("Sign in anonymously succeeded!");
            }
            catch (RequestFailedException exception)
            {
                Debug.LogError($"Sign in anonymously failed with error code: {exception.ErrorCode}");
            }
        }
    }
}
