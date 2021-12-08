##Quick Start Guide

This will show how to setup your own project to use the Authentication SDK

###Link your project

To use the Unity Authentication service, you’re required to link your project to a cloud project in the Unity Editor through a project ID. Follow these steps to get your project ID.


1. In the Unity Editor menu, go to `Edit > Project Settings` to open the `Services` tab.
2. If you’re not already signed in with your Unity ID, either create a new Unity ID or sign-in.
3. If you want to create a new project, select your organization and click `Create`.
4. If you want to link to an existing project, click `I already have a Unity Project ID`. Then, select your organization and project from the dropdown list, and click `Link`.

Now you can find your project ID from the `Settings` tab in the `Services` window.

###Initialize the Unity Services SDK

To implement Unity Authentication in your game, after installing the Authentication SDK, initialize all the Unity Services SDKs included in the project following this code snippet:

```
using Unity.Services.Core;
using Unity.Services.Authentication;

// Game developer code
public class MyClass : MonoBehaviour
{
    async void Start()
    {
        // UnityServices.InitializeAsync() will initialize all services that are subscribed to Core.
        await UnityServices.InitializeAsync();
        Debug.Log(UnityServices.State);
    }
}
```

###Authentication Event Registration

To be updated about the status of your user, register a function to the SignedIn, SignInFailed, SignedOut and Expired event handlers.

```
// Setup authentication event handlers if desired
void SetupEvents()
{
    AuthenticationService.Instance.SignedIn += () =>
    {
         // Shows how to get a playerID
        Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

        // Shows how to get an access token
        Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");
    };

    AuthenticationService.Instance.SignInFailed += (err) =>
    {
        Debug.LogError(err);
    };

    AuthenticationService.Instance.SignedOut += () =>
    {
        Debug.Log("Player signed out.");
    };
	
    AuthenticationService.Instance.Expired += () =>
    {
        Debug.Log("Player session expired.");
    };
}
```

###How to use Anonymous Sign-in

Anonymous sign-in creates a new user for the game session without any input from the player and is a quick way for a player to get started with your game. The following code sample shows how to set up the ability for players to sign in anonymously in your game and get your access token.

If a player has already signed in before, the `SignInAnonymouslyAsync()` recovers the existing login of a user whether they signed in anonymously or through a social account. 

```
async Task SignInAnonymouslyAsync()
{
    try
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log("Sign in anonymously succeeded!");
    }
    catch (AuthenticationException ex)
    {
        // Compare error code to AuthenticationErrorCodes
        // Notify the player with the proper error message
        Debug.LogException(ex);
    }
    catch (RequestFailedException exception)
    {
        // Compare error code to CommonErrorCodes
        // Notify the player with the proper error message
        Debug.LogException(ex);
     }
}
```
