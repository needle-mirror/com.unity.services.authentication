using System.Threading.Tasks;

namespace Unity.Services.Authentication.PlayerAccounts
{
    /// <summary>
    /// Provides a set of utility functions for interacting with a web browser.
    /// </summary>
    interface IBrowserUtils
    {
        /// <summary>
        /// Launches the specified URL in a web browser.
        /// </summary>
        /// <param name="url">The URL to launch.</param>
        Task LaunchUrlAsync(string url);

        /// <summary>
        /// Perform binding if required
        /// </summary>
        /// <returns>Returns if successful</returns>
        bool Bind();

        /// <summary>
        /// Dismisses the browser.
        /// </summary>
        void Dismiss();

        /// <summary>
        /// Gets the redirect uri
        /// </summary>
        /// <returns>The uri</returns>
        string GetRedirectUri();
    }
}
