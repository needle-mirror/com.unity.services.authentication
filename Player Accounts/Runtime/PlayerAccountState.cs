namespace Unity.Services.Authentication.PlayerAccounts
{
    enum PlayerAccountState
    {
        SignedOut,
        SigningIn,
        Authorized,
        Refreshing,
        Expired
    }
}
