using Unity.Services.Core.Editor;
using Unity.Services.Core.Editor.OrganizationHandler;
using UnityEditor;

namespace Unity.Services.Authentication.PlayerAccounts.Editor
{
    class PlayerAccountEditorService : IEditorGameService
    {
        /// <summary>
        /// Name of the service
        /// Used for error handling and service fetching
        /// </summary>
        public string Name => "Unity Player Accounts";

        /// <summary>
        /// Identifier for the service
        /// Used when registering and fetching the service
        /// </summary>
        public IEditorGameServiceIdentifier Identifier { get; } = new PlayerAccountIdentifier();

        /// <summary>
        /// Flag used to determine whether COPPA Compliance should be adhered to
        /// for this service
        /// </summary>
        public bool RequiresCoppaCompliance { get; } = false;

        /// <summary>
        /// Flag used to determine whether this service has a dashboard
        /// </summary>
        public bool HasDashboard { get; } = true;

        /// <summary>
        /// Getter for the formatted dashboard url
        /// </summary>
        /// <returns>The dashboard url</returns>
        public string GetFormattedDashboardUrl()
        {
            if (IsConfigured())
            {
                return $"https://cloud.unity3d.com/organizations/{GetOrganizationId()}/projects/{GetProjectId()}/player-authentication/identity-providers";
            }

            return $"https://cloud.unity3d.com/player-authentication";
        }

        /// <summary>
        /// The enabler which allows the service to toggle on/off
        /// Can be set to null, in which case there would be no toggle
        /// </summary>
        public IEditorGameServiceEnabler Enabler { get; } = null;

        static bool IsConfigured()
        {
            return !string.IsNullOrEmpty(GetOrganizationId()) && !string.IsNullOrEmpty(GetProjectId());
        }

        static string GetOrganizationId()
        {
            return OrganizationProvider.Organization.Key;
        }

        static string GetProjectId()
        {
            return CloudProjectSettings.projectId;
        }
    }
}
