using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Core;

namespace Unity.Services.Authentication
{
    partial class AuthenticationServiceInternal
    {
        public Task SignInWithUsernamePasswordAsync(string username, string password)
        {
            return SignInWithUsernamePasswordRequestAsync(BuildUsernamePasswordRequest(username, password));
        }

        public Task SignUpWithUsernamePasswordAsync(string username, string password)
        {
            return SignUpWithUsernamePasswordRequestAsync(BuildUsernamePasswordRequest(username, password));
        }

        public Task AddUsernamePasswordAsync(string username, string password)
        {
            return AddUsernamePasswordRequestAsync(BuildUsernamePasswordRequest(username, password));
        }

        public Task UpdatePasswordAsync(string currentPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
            {
                throw ExceptionHandler.BuildInvalidCredentialsException();
            }

            return UpdatePasswordRequestAsync(new UpdatePasswordRequest
            {
                Password = currentPassword,
                NewPassword = newPassword
            });
        }

        internal Task SignInWithUsernamePasswordRequestAsync(UsernamePasswordRequest request, bool enableRefresh = true)
        {
            if (State == AuthenticationState.SignedOut || State == AuthenticationState.Expired)
            {
                return HandleSignInRequestAsync(() => NetworkClient.SignInWithUsernamePasswordAsync(request), enableRefresh);
            }

            var exception = ExceptionHandler.BuildClientInvalidStateException(State);
            SendSignInFailedEvent(exception, false);
            return Task.FromException(exception);
        }

        internal Task SignUpWithUsernamePasswordRequestAsync(UsernamePasswordRequest request, bool enableRefresh = true)
        {
            if (State == AuthenticationState.SignedOut || State == AuthenticationState.Expired)
            {
                return HandleSignInRequestAsync(() => NetworkClient.SignUpWithUsernamePasswordAsync(request), enableRefresh);
            }

            var exception = ExceptionHandler.BuildClientInvalidStateException(State);
            SendSignInFailedEvent(exception, false);
            return Task.FromException(exception);
        }

        internal async Task AddUsernamePasswordRequestAsync(UsernamePasswordRequest request)
        {
            if (IsAuthorized)
            {
                try
                {
                    var response = await NetworkClient.AddUsernamePasswordAsync(request);
                    PlayerInfo.Username = response.User?.Username;
                    return;
                }
                catch (WebRequestException e)
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

        internal Task UpdatePasswordRequestAsync(UpdatePasswordRequest request, bool enableRefresh = true)
        {
            if (IsAuthorized)
            {
                // Player is signed in, update the credentials (sessionToken, accessToken)
                return HandleUpdatePasswordRequestAsync(() => NetworkClient.UpdatePasswordAsync(request), enableRefresh);
            }
            else
            {
                var exception = ExceptionHandler.BuildClientInvalidStateException(State);
                return Task.FromException(exception);
            }
        }

        internal async Task HandleUpdatePasswordRequestAsync(Func<Task<SignInResponse>> updatePasswordRequest, bool enableRefresh = true)
        {
            try
            {
                CompleteSignIn(await updatePasswordRequest(), enableRefresh);
            }
            catch (RequestFailedException e)
            {
                SendUpdatePasswordFailedEvent(e, false);
                throw;
            }
            catch (WebRequestException e)
            {
                var authException = ExceptionHandler.ConvertException(e);

                if (authException.ErrorCode == AuthenticationErrorCodes.InvalidSessionToken)
                {
                    SessionTokenComponent.Clear();
                    Logger.Log($"The session token is invalid and has been cleared. The associated account is no longer accessible through this login method.");
                }

                SendUpdatePasswordFailedEvent(authException, false);
                throw authException;
            }
        }

        UsernamePasswordRequest BuildUsernamePasswordRequest(string username, string password)
        {
            if (!ValidateCredentials(username, password))
            {
                throw ExceptionHandler.BuildInvalidCredentialsException();
            }

            return new UsernamePasswordRequest
            {
                Username = username,
                Password = password
            };
        }

        void SendUpdatePasswordFailedEvent(RequestFailedException exception, bool forceSignOut)
        {
            UpdatePasswordFailed?.Invoke(exception);
            if (forceSignOut)
            {
                SignOut();
            }
        }

        bool ValidateCredentials(string username, string password)
        {
            return !string.IsNullOrEmpty(username)
                && !string.IsNullOrEmpty(password);
        }
    }
}
