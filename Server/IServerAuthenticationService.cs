using System;
using System.Threading.Tasks;
using Unity.Services.Core;

namespace Unity.Services.Authentication.Server
{
    /// <summary>
    /// The functions for the Server Authentication service.
    /// </summary>
    public interface IServerAuthenticationService
    {
        /// <summary>
        /// Invoked when an authorization attempt has completed successfully.
        /// </summary>
        event Action Authorized;

        /// <summary>
        /// Invoked when an access token expires.
        /// </summary>
        event Action Expired;

        /// <summary>
        /// Invoked when a sign-in attempt has failed. The reason for failure is passed as the parameter
        /// <see cref="ServerAuthenticationException"/>.
        /// </summary>
        event Action<ServerAuthenticationException> AuthorizationFailed;

        /// <summary>
        /// The current state of the service
        /// </summary>
        ServerAuthenticationState State { get; }

        /// <summary>
        /// Validates that the state is authorized.
        /// </summary>
        bool IsAuthorized { get; }

        /// <summary>
        /// Returns the current access token, otherwise null.
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        /// Trusted sign-in using service account credentials
        /// </summary>
        /// <param name="apiKeyIdentifier">The service account key id</param>
        /// <param name="apiKeySecret">The service account key secret</param>
        /// <returns>Task for the operation</returns>
        /// <exception cref="ServerAuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// </exception>
        Task SignInWithServiceAccountAsync(string apiKeyIdentifier, string apiKeySecret);

        /// <summary>
        /// Retrieve a token to authorize server operations from a hosted server.
        /// Must be running on a multiplay server or with the server local proxy activated.
        /// </summary>
        /// <returns>Task for the operation</returns>
        /// <exception cref="ServerAuthenticationException">
        /// The task fails with the exception when the task cannot complete successfully.
        /// </exception>
        Task SignInFromServerAsync();

        /// <summary>
        /// Clears the access token and authorization state.
        /// </summary>
        void ClearCredentials();
    }
}
