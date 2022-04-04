using System.Threading.Tasks;

namespace Unity.Services.Authentication.Editor
{
    interface IAuthenticationAdminNetworkClient
    {
        Task<TokenExchangeResponse> ExchangeTokenAsync(string token);
        Task<GetIdDomainResponse> GetDefaultIdDomainAsync(string token);
        Task<IdProviderResponse> CreateIdProviderAsync(CreateIdProviderRequest body, string idDomain, string token);
        Task<ListIdProviderResponse> ListIdProviderAsync(string idDomain, string token);
        Task<IdProviderResponse> UpdateIdProviderAsync(UpdateIdProviderRequest body, string idDomain, string type, string token);
        Task<IdProviderResponse> EnableIdProviderAsync(string idDomain, string type, string token);
        Task<IdProviderResponse> DisableIdProviderAsync(string idDomain, string type, string token);
        Task<IdProviderResponse> DeleteIdProviderAsync(string idDomain, string type, string token);
    }
}
