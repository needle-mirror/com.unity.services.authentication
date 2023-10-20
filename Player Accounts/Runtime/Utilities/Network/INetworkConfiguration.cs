namespace Unity.Services.Authentication.PlayerAccounts
{
    interface INetworkConfiguration
    {
        int Retries { get; }
        int Timeout { get; }
    }
}
