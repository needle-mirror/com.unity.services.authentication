using System.Threading.Tasks;

namespace Unity.Services.Authentication.Editor
{
    interface IAuthenticationAdminClient
    {
        /// <summary>
        /// Get the services gateway token.
        /// </summary>
        string GatewayToken { get; }

        /// <summary>
        /// Get the ID domain associated with the project.
        /// </summary>
        /// <returns>Task with the id domain as the result.</returns>
        Task<string> GetIDDomainAsync();

        /// <summary>
        /// Lists all ID providers created for the organization's specified ID domain
        /// </summary>
        /// <param name="idDomain">The ID domain ID</param>
        /// <returns>Task with the list of ID Providers configured in the ID domain.</returns>
        Task<ListIdProviderResponse> ListIdProvidersAsync(string idDomain);

        /// <summary>
        /// Create a new ID provider for the organization's specified ID domain
        /// </summary>
        /// <param name="idDomain">The ID domain ID</param>
        /// <param name="request">The ID provider to create.</param>
        /// <returns>Task with the ID Provider created.</returns>
        Task<IdProviderResponse> CreateIdProviderAsync(string idDomain, CreateIdProviderRequest request);

        /// <summary>
        /// Update an ID provider for the organization's specified ID domain
        /// </summary>
        /// <param name="idDomain">The ID domain ID</param>
        /// <param name="request">The ID provider to create.</param>
        /// <returns>Task with the ID Provider updated.</returns>
        Task<IdProviderResponse> UpdateIdProviderAsync(string idDomain, string type, UpdateIdProviderRequest request);

        /// <summary>
        /// Enable an ID provider for the organization's specified ID domain
        /// </summary>
        /// <param name="idDomain">The ID domain ID</param>
        /// <param name="type">The type of the ID provider.</param>
        /// <returns>Task with the ID Provider updated.</returns>
        Task<IdProviderResponse> EnableIdProviderAsync(string idDomain, string type);

        /// <summary>
        /// Disable an ID provider for the organization's specified ID domain
        /// </summary>
        /// <param name="idDomain">The ID domain ID</param>
        /// <param name="type">The type of the ID provider.</param>
        /// <returns>Task with the ID Provider updated.</returns>
        Task<IdProviderResponse> DisableIdProviderAsync(string idDomain, string type);

        /// <summary>
        /// Delete a specific ID provider from the organization's specified ID domain
        /// </summary>
        /// <param name="idDomain">The ID domain ID</param>
        /// <param name="type">The type of the ID provider.</param>
        /// <returns>Task with the deleted id provider info.</returns>
        Task<IdProviderResponse> DeleteIdProviderAsync(string idDomain, string type);
    }
}
