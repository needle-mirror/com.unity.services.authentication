namespace Unity.Services.Authentication
{
    interface INetworkConfiguration
    {
        int Retries { get; }
        int Timeout { get; }
    }
}
