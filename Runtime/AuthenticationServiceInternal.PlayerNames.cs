using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication.Generated;
using Unity.Services.Authentication.Shared;

namespace Unity.Services.Authentication
{
    partial class AuthenticationServiceInternal
    {
        public string PlayerName => PlayerNameComponent.PlayerName;

        public async Task<string> GetPlayerNameAsync(bool autoGenerate = true)
        {
            if (IsAuthorized)
            {
                try
                {
                    PlayerNamesApi.Configuration.AccessToken = AccessTokenComponent.AccessToken;
                    var response = await PlayerNamesApi.GetNameAsync(PlayerId, autoGenerate);
                    var player = response.Data;
                    PlayerNameComponent.PlayerName = player.Name;
                    return player.Name;
                }
                catch (ApiException e)
                {
                    if (e.Response.StatusCode == 404) // HttpStatusCode.NotFound
                    {
                        PlayerNameComponent.Clear();
                        return null;
                    }

                    throw ExceptionHandler.ConvertException(e);
                }
                catch (Exception e)
                {
                    throw ExceptionHandler.BuildUnknownException(e.Message);
                }
            }
            else
            {
                throw ExceptionHandler.BuildClientInvalidStateException(State);
            }
        }

        public async Task<string> UpdatePlayerNameAsync(string playerName)
        {
            if (IsAuthorized)
            {
                if (string.IsNullOrWhiteSpace(playerName) || playerName.Any(char.IsWhiteSpace))
                {
                    throw ExceptionHandler.BuildInvalidPlayerNameException();
                }

                try
                {
                    PlayerNamesApi.Configuration.AccessToken = AccessTokenComponent.AccessToken;
                    var response = await PlayerNamesApi.UpdateNameAsync(PlayerId, new UpdateNameRequest(playerName));
                    var playerNameResult = response.Data?.Name;

                    if (string.IsNullOrWhiteSpace(playerNameResult))
                    {
                        throw ExceptionHandler.BuildUnknownException("Invalid player name response");
                    }

                    PlayerNameComponent.PlayerName = playerNameResult;
                    return playerNameResult;
                }
                catch (ApiException e)
                {
                    throw ExceptionHandler.ConvertException(e);
                }
                catch (Exception e)
                {
                    throw ExceptionHandler.BuildUnknownException(e.Message);
                }
            }
            else
            {
                throw ExceptionHandler.BuildClientInvalidStateException(State);
            }
        }

    }
}
