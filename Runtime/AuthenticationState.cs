namespace Unity.Services.Authentication
{
    enum AuthenticationState
    {
        SignedOut,
        SigningIn,
        Authorized,
        Refreshing,
        Expired
    }
}
