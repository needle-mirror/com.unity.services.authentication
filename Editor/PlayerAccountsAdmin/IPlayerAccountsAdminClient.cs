using System.Threading.Tasks;

namespace Unity.Services.Authentication.Editor
{
    /// <summary>
    /// Manages Unity Player Accounts for the Editor's linked project: its one Player Accounts client, the matching
    /// Unity Player Accounts sign-in provider in Authentication, and the game's own social provider apps.
    /// Every method throws <see cref="PlayerAccountsAdminException"/> when the call fails.
    /// </summary>
    interface IPlayerAccountsAdminClient
    {
        /// <summary>The linked Unity Cloud project the calls target.</summary>
        string ProjectId { get; }

        /// <summary>
        /// Starts creating the project's Player Accounts client, or updating it when it already exists, which turns
        /// Player Accounts on for the project. The service accepts the request and returns nothing: the client's id
        /// appears on the Unity Player Accounts sign-in provider once it is ready.
        /// </summary>
        Task CreateOrUpdateClientAsync(PlayerAccountsClientRequest request);

        Task<PlayerAccountsClientResponse> GetClientAsync(string clientId);

        /// <summary>Changes only the request's non-null fields; an empty link removes it.</summary>
        Task<PlayerAccountsClientResponse> UpdateClientAsync(string clientId, PlayerAccountsClientRequest request);

        /// <summary>
        /// The project's Unity Player Accounts sign-in provider in Authentication (the <c>unity</c> entry), whose
        /// client id is the Player Accounts client's. Null when Player Accounts is not set up for the project.
        /// </summary>
        Task<IdProviderResponse> GetUnitySignInProviderAsync();

        Task<IdProviderResponse> EnableUnitySignInProviderAsync();

        Task<PlayerAccountsIdentityProviderResponse[]> ListIdentityProvidersAsync();

        Task<PlayerAccountsIdentityProviderResponse> CreateIdentityProviderAsync(CreatePlayerAccountsIdentityProviderRequest request);

        Task<PlayerAccountsIdentityProviderResponse> UpdateIdentityProviderAsync(string id, UpdatePlayerAccountsIdentityProviderRequest request);

        Task DeleteIdentityProviderAsync(string id);
    }
}
