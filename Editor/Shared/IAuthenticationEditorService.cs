using Unity.Services.Authentication.Internal;

namespace Unity.Services.Authentication.Editor.Shared
{
    interface IAuthenticationEditorService : IAuthenticationService
    {
        /// <summary>
        /// Component for retrieving the access token
        /// </summary>
        IAccessToken AccessTokenComponent { get; }

        /// <summary>
        /// Component for retrieving the environment id
        /// </summary>
        IEnvironmentId EnvironmentIdComponent { get; }

        /// <summary>
        /// Component for retrieving the player id
        /// </summary>
        IPlayerId PlayerIdComponent { get; }

        /// <summary>
        /// Component for retrieving the player name
        /// </summary>
        IPlayerName PlayerNameComponent { get; }
    }
}
