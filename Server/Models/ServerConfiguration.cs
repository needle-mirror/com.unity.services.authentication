using Unity.Services.Authentication.Server.Shared;

namespace Unity.Services.Authentication.Server
{
    interface IServerConfiguration
    {
        int Retries { get; }
        int Timeout { get; }

        void SetServiceAccount(string apiKeyIdentifier, string apiKeySecret);
    }

    class ServerConfiguration : IServerConfiguration
    {
        const int k_DefaultRetries = 2;
        const int k_DefaultTimeout = 5;

        public ApiConfiguration GatewayConfiguration { get; }
        public ApiConfiguration ProxyConfiguration { get; }

        public int Retries { get; set; } = k_DefaultRetries;
        public int Timeout { get; set; } = k_DefaultTimeout;

        public ServerConfiguration(ApiConfiguration gatewayConfiguration, ApiConfiguration proxyConfiguration)
        {
            GatewayConfiguration = gatewayConfiguration;
            ProxyConfiguration = proxyConfiguration;
        }

        public void SetServiceAccount(string apiKeyIdentifier, string apiKeySecret)
        {
            GatewayConfiguration.Username = apiKeyIdentifier;
            GatewayConfiguration.Password = apiKeySecret;
        }
    }
}
