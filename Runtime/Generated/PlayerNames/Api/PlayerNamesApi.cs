
//-----------------------------------------------------------------------------
// <auto-generated>
//     Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Mime;
using Unity.Services.Authentication.Shared;

namespace Unity.Services.Authentication.Generated
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    internal interface IPlayerNamesApi : IApiAccessor
    {
        /// <summary>
        /// Get a player&#39;s username.
        /// </summary>
        /// <remarks>
        /// Get a player&#39;s username. The &#39;/me&#39; endpoint can be used to get the username of the calling player.
        /// </remarks>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="playerId">The player&#39;s ID.</param>
        /// <param name="autoGenerate">Indicates if a player without a name should have one auto generated or not. Defaults to true. (optional)</param>
        /// <param name="showMetadata">If true, returns additional metadata like &#39;autoGenerated&#39; with records. Defaults to false. (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Player</returns>
        System.Threading.Tasks.Task<ApiResponse<Player>> GetNameAsync(string playerId, bool? autoGenerate = default(bool?), bool? showMetadata = default(bool?), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Update a player&#39;s username.
        /// </summary>
        /// <remarks>
        /// Update a player&#39;s username, or create it if it doesn&#39;t exist. White space is not allowed in the username, and a random numeric suffix will automatically be added to it.
        /// </remarks>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="playerId">The player&#39;s ID.</param>
        /// <param name="updateNameRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Player</returns>
        System.Threading.Tasks.Task<ApiResponse<Player>> UpdateNameAsync(string playerId, UpdateNameRequest updateNameRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    internal partial class PlayerNamesApi : IPlayerNamesApi
    {
        /// <summary>
        /// The client for accessing this underlying API asynchronously.
        /// </summary>
        public IApiClient Client { get; }

        /// <summary>
        /// Gets the configuration object
        /// </summary>
        /// <value>An instance of the Configuration</value>
        public IApiConfiguration Configuration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerNamesApi"/> class
        /// using a Configuration object and client instance.
        /// </summary>
        /// <param name="apiClient">The client interface for synchronous API access.</param>
        public PlayerNamesApi(IApiClient apiClient)
        {
            if (apiClient == null) throw new ArgumentNullException("apiClient");

            this.Client = apiClient;
            this.Configuration = new ApiConfiguration()
            {
                BasePath = "https://social.services.api.unity.com/v1"
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerNamesApi"/> class
        /// using a Configuration object and client instance.
        /// </summary>
        /// <param name="apiClient">The client interface for synchronous API access.</param>
        /// <param name="apiConfiguration">The configuration object.</param>
        public PlayerNamesApi(IApiClient apiClient, IApiConfiguration apiConfiguration)
        {
            if (apiClient == null) throw new ArgumentNullException("apiClient");
            if (apiConfiguration == null) throw new ArgumentNullException("apiConfiguration");

            this.Client = apiClient;
            this.Configuration = apiConfiguration;
        }

        /// <summary>
        /// Gets the base path of the API client.
        /// </summary>
        /// <returns>The base path</returns>
        public string GetBasePath()
        {
            return this.Configuration.BasePath;
        }

        /// <summary>
        /// Get a player&#39;s username. Get a player&#39;s username. The &#39;/me&#39; endpoint can be used to get the username of the calling player.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="playerId">The player&#39;s ID.</param>
        /// <param name="autoGenerate">Indicates if a player without a name should have one auto generated or not. Defaults to true. (optional)</param>
        /// <param name="showMetadata">If true, returns additional metadata like &#39;autoGenerated&#39; with records. Defaults to false. (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Player</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Player>> GetNameAsync(string playerId, bool? autoGenerate = default(bool?), bool? showMetadata = default(bool?), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'playerId' is set
            if (playerId == null)
            {
                throw new ApiException(ApiExceptionType.InvalidParameters, "Missing required parameter 'playerId' when calling PlayerNamesApi->GetName");
            }

            ApiRequestOptions localRequestOptions = new ApiRequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localContentType = ApiUtils.SelectHeaderContentType(_contentTypes);
            if (localContentType != null)
            {
                localRequestOptions.HeaderParameters.Add("Content-Type", localContentType);
            }

            var localAccept = ApiUtils.SelectHeaderAccept(_accepts);
            if (localAccept != null)
            {
                localRequestOptions.HeaderParameters.Add("Accept", localAccept);
            }

            localRequestOptions.PathParameters.Add("playerId", ApiUtils.ParameterToString(Configuration, playerId)); // path parameter
            if (autoGenerate != null)
            {
                localRequestOptions.QueryParameters.Add(ApiUtils.ParameterToMultiMap(Configuration, "", "autoGenerate", autoGenerate));
            }
            if (showMetadata != null)
            {
                localRequestOptions.QueryParameters.Add(ApiUtils.ParameterToMultiMap(Configuration, "", "showMetadata", showMetadata));
            }

            localRequestOptions.Operation = "PlayerNamesApi.GetName";

            // authentication (Client) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }

            // make the HTTP request
            return await this.Client.GetAsync<Player>("/names/{playerId}", localRequestOptions, this.Configuration, cancellationToken);
        }
        /// <summary>
        /// Update a player&#39;s username. Update a player&#39;s username, or create it if it doesn&#39;t exist. White space is not allowed in the username, and a random numeric suffix will automatically be added to it.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="playerId">The player&#39;s ID.</param>
        /// <param name="updateNameRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Player</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Player>> UpdateNameAsync(string playerId, UpdateNameRequest updateNameRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'playerId' is set
            if (playerId == null)
            {
                throw new ApiException(ApiExceptionType.InvalidParameters, "Missing required parameter 'playerId' when calling PlayerNamesApi->UpdateName");
            }
            // verify the required parameter 'updateNameRequest' is set
            if (updateNameRequest == null)
            {
                throw new ApiException(ApiExceptionType.InvalidParameters, "Missing required parameter 'updateNameRequest' when calling PlayerNamesApi->UpdateName");
            }

            ApiRequestOptions localRequestOptions = new ApiRequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localContentType = ApiUtils.SelectHeaderContentType(_contentTypes);
            if (localContentType != null)
            {
                localRequestOptions.HeaderParameters.Add("Content-Type", localContentType);
            }

            var localAccept = ApiUtils.SelectHeaderAccept(_accepts);
            if (localAccept != null)
            {
                localRequestOptions.HeaderParameters.Add("Accept", localAccept);
            }

            localRequestOptions.PathParameters.Add("playerId", ApiUtils.ParameterToString(Configuration, playerId)); // path parameter
            localRequestOptions.Data = updateNameRequest;

            localRequestOptions.Operation = "PlayerNamesApi.UpdateName";

            // authentication (Client) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }

            // make the HTTP request
            return await this.Client.PostAsync<Player>("/names/{playerId}", localRequestOptions, this.Configuration, cancellationToken);
        }
    }
}
