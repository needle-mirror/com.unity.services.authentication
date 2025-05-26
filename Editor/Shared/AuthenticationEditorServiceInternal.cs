using Unity.Services.Authentication.Generated;
using Unity.Services.Authentication.Internal;
using Unity.Services.Core.Environments.Internal;
using Unity.Services.Core.Scheduler.Internal;

namespace Unity.Services.Authentication.Editor.Shared
{
    internal static partial class AuthenticationEditorService
    {
        class AuthenticationEditorServiceInternal : AuthenticationServiceInternal, IAuthenticationEditorService
        {
            IAccessToken IAuthenticationEditorService.AccessTokenComponent => AccessTokenComponent;
            IEnvironmentId IAuthenticationEditorService.EnvironmentIdComponent => EnvironmentIdComponent;
            IPlayerId IAuthenticationEditorService.PlayerIdComponent => PlayerIdComponent;
            IPlayerName IAuthenticationEditorService.PlayerNameComponent => PlayerNameComponent;

            internal AuthenticationEditorServiceInternal(
                IAuthenticationSettings settings,
                IAuthenticationNetworkClient networkClient,
                IPlayerNamesApi playerNamesApi,
                IProfile profile,
                IJwtDecoder jwtDecoder,
                IAuthenticationCache cache,
                IActionScheduler scheduler,
                IAuthenticationMetrics metrics,
                AccessTokenComponent accessToken,
                EnvironmentIdComponent environmentId,
                PlayerIdComponent playerId,
                PlayerNameComponent playerName,
                SessionTokenComponent sessionToken,
                IEnvironments environment)
                : base(settings,
                    networkClient,
                    playerNamesApi,
                    profile,
                    jwtDecoder,
                    cache,
                    scheduler,
                    metrics,
                    accessToken,
                    environmentId,
                    playerId,
                    playerName,
                    sessionToken,
                    environment)
            {
            }
        }
    }
}
