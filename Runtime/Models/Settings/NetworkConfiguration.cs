namespace Unity.Services.Authentication
{
    class NetworkConfiguration : INetworkConfiguration
    {
        const int k_DefaultRetries = 2;
        const int k_DefaultTimeout = 5;

        public int Retries { get; set; } = k_DefaultRetries;
        public int Timeout { get; set; } = k_DefaultTimeout;
    }
}
