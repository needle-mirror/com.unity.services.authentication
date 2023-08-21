using System.Threading.Tasks;

namespace Unity.Services.Authentication.Editor
{
    interface IAuthenticationAdminNetworkClient
    {
        Task<TokenExchangeResponse> ExchangeTokenAsync(string token);
        Task<IdProviderResponse> CreateIdProviderAsync(CreateIdProviderRequest body, string projectId, string token);
        Task<ListIdProviderResponse> ListIdProviderAsync(string projectId, string token);
        Task<IdProviderResponse> UpdateIdProviderAsync(UpdateIdProviderRequest body, string projectId, string type, string token);
        Task<IdProviderResponse> EnableIdProviderAsync(string projectId, string type, string token);
        Task<IdProviderResponse> DisableIdProviderAsync(string projectId, string type, string token);
        Task<IdProviderResponse> DeleteIdProviderAsync(string projectId, string type, string token);
    }
}
