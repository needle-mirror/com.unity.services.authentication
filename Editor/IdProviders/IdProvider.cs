using System;

namespace Unity.Services.Authentication.Editor
{
    class IdProvider
    {
        public string Type { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public OpenIDConfig  OidcConfig { get; set; }
        public bool Disabled { get; set; }
        public bool New { get; set; }

        public IdProvider() {}

        public IdProvider(IdProviderResponse response)
        {
            Type = response.Type;
            ClientId = response.ClientId;
            ClientSecret = response.ClientSecret;
            OidcConfig = response.OidcConfig;
            Disabled = response.Disabled;
            New = false;
        }

        public IdProvider Clone()
        {
            return new IdProvider()
            {
                Type = Type,
                ClientId = ClientId,
                ClientSecret = ClientSecret,
                OidcConfig = OidcConfig,
                Disabled = Disabled,
                New = New,
            };
        }
    }
}
