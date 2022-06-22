using Unity.Services.Core.Editor;
using Unity.Services.Core.Editor.OrganizationHandler;
using UnityEditor;

namespace Unity.Services.Authentication.Editor
{
    class AuthenticationEditorService : IEditorGameService
    {
        /// <summary>
        /// Name of the service
        /// Used for error handling and service fetching
        /// </summary>
        public string Name => "Authentication Service";

        /// <summary>
        /// Identifier for the service
        /// Used when registering and fetching the service
        /// </summary>
        public IEditorGameServiceIdentifier Identifier { get; } = new AuthenticationIdentifier();

        /// <summary>
        /// Flag used to determine whether COPPA Compliance should be adhered to
        /// for this service
        /// </summary>
        public bool RequiresCoppaCompliance => false;

        /// <summary>
        /// Flag used to determine whether this service has a dashboard
        /// </summary>
        public bool HasDashboard => true;

        /// <summary>
        /// The enabler which allows the service to toggle on/off
        /// Can be set to null, in which case there would be no toggle
        /// </summary>
        public IEditorGameServiceEnabler Enabler { get; } = null;

        /// <summary>
        /// Getter for the formatted dashboard url
        /// If <see cref="HasDashboard"/> is false, this field only need return null or empty string
        /// </summary>
        /// <returns>The formatted URL</returns>
        public string GetFormattedDashboardUrl()
        {
            if (AuthenticationAdminClientManager.IsConfigured())
            {
                return $"https://dashboard.unity3d.com/organizations/{AuthenticationAdminClientManager.GetOrganizationId()}/projects/{AuthenticationAdminClientManager.GetProjectId()}/player-authentication/identity-providers";
            }
            return $"https://dashboard.unity3d.com/player-authentication";
        }
    }
}
