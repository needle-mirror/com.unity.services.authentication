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
        /// Lists all ID providers created for the organization's specified project ID
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <returns>Task with the list of ID Providers configured in the ID domain.</returns>
        Task<ListIdProviderResponse> ListIdProvidersAsync(string projectId);

        /// <summary>
        /// Create a new ID provider for the organization's specified project ID
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <param name="request">The ID provider to create.</param>
        /// <returns>Task with the ID Provider created.</returns>
        Task<IdProviderResponse> CreateIdProviderAsync(string projectId, CreateIdProviderRequest request);

        /// <summary>
        /// Update an ID provider for the organization's specified project ID
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <param name="request">The ID provider to create.</param>
        /// <returns>Task with the ID Provider updated.</returns>
        Task<IdProviderResponse> UpdateIdProviderAsync(string projectId, string type, UpdateIdProviderRequest request);

        /// <summary>
        /// Enable an ID provider for the organization's specified project ID
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <param name="type">The type of the ID provider.</param>
        /// <returns>Task with the ID Provider updated.</returns>
        Task<IdProviderResponse> EnableIdProviderAsync(string projectId, string type);

        /// <summary>
        /// Disable an ID provider for the organization's specified project ID
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <param name="type">The type of the ID provider.</param>
        /// <returns>Task with the ID Provider updated.</returns>
        Task<IdProviderResponse> DisableIdProviderAsync(string projectId, string type);

        /// <summary>
        /// Delete a specific ID provider from the organization's specified project ID
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <param name="type">The type of the ID provider.</param>
        /// <returns>Task with the deleted id provider info.</returns>
        Task<IdProviderResponse> DeleteIdProviderAsync(string projectId, string type);
    }
}
