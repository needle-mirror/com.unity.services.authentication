using System.Threading.Tasks;

namespace Unity.Services.Authentication
{
    interface IAuthenticationNetworkClient
    {
        Task<WellKnownKeysResponse> GetWellKnownKeysAsync();
        Task<SignInResponse> SignInAnonymouslyAsync();
        Task<SignInResponse> SignInWithSessionTokenAsync(string token);
        Task<SignInResponse> SignInWithExternalTokenAsync(string idProvider, SignInWithExternalTokenRequest externalToken);
        Task<LinkResponse> LinkWithExternalTokenAsync(string idProvider, LinkWithExternalTokenRequest externalToken);
        Task<UnlinkResponse> UnlinkExternalTokenAsync(string idProvider, UnlinkRequest request);
        Task<PlayerInfoResponse> GetPlayerInfoAsync(string playerId);
        Task DeleteAccountAsync(string playerId);
    }
}
