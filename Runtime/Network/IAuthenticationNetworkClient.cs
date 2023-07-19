using System.Threading.Tasks;

namespace Unity.Services.Authentication
{
    interface IAuthenticationNetworkClient
    {
        Task<SignInResponse> SignInAnonymouslyAsync();
        Task<SignInResponse> SignInWithSessionTokenAsync(string token);
        Task<SignInResponse> SignInWithExternalTokenAsync(string idProvider, SignInWithExternalTokenRequest externalToken);
        Task<LinkResponse> LinkWithExternalTokenAsync(string idProvider, LinkWithExternalTokenRequest externalToken);
        Task<UnlinkResponse> UnlinkExternalTokenAsync(string idProvider, UnlinkRequest request);
        Task<PlayerInfoResponse> GetPlayerInfoAsync(string playerId);
        Task DeleteAccountAsync(string playerId);
        Task<SignInResponse> SignInWithUsernamePasswordAsync(UsernamePasswordRequest credentials);
        Task<SignInResponse> SignUpWithUsernamePasswordAsync(UsernamePasswordRequest credentials);
        Task<SignInResponse> AddUsernamePasswordAsync(UsernamePasswordRequest credentials);
        Task<SignInResponse> UpdatePasswordAsync(UpdatePasswordRequest credentials);
    }
}
