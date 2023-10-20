using System;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Services.Authentication
{
    partial class AuthenticationServiceInternal
    {
        internal string CodeLinkSessionId { get; set; }
        internal string CodeVerifier { get; set; }


        public async Task<SignInCodeInfo> GenerateSignInCodeAsync(string identifier = null)
        {
            if (State != AuthenticationState.SignedOut && State != AuthenticationState.Expired)
            {
                var exception = ExceptionHandler.BuildClientInvalidStateException(State);
                SendSignInFailedEvent(exception, false);
                throw exception;
            }

            var challengeGenerator = new CodeChallengeGenerator();
            var codeVerifier = challengeGenerator.GenerateCode();
            var codeChallenge = CodeChallengeGenerator.S256EncodeChallenge(codeVerifier);

            var generateCodeRequest = new GenerateSignInCodeRequest
            {
                Identifier = identifier,
                CodeChallenge = codeChallenge
            };

            var generateCodeResponse = await NetworkClient.GenerateSignInCodeAsync(generateCodeRequest);
            var info = new SignInCodeInfo()
            {
                SignInCode = generateCodeResponse.SignInCode,
                Expiration = generateCodeResponse.Expiration
            };

            CodeLinkSessionId = generateCodeResponse.CodeLinkSessionId;
            CodeVerifier = codeVerifier;

            SignInCodeReceived?.Invoke(info);
            return info;
        }

        public async Task SignInWithCodeAsync(bool usePolling = false, CancellationToken cancellationToken = default)
        {
            if (CodeVerifier == null || CodeLinkSessionId == null)
            {
                throw ExceptionHandler.BuildUnknownException("SignInWithCodeAsync failed: No sign-in code has been generated. Ensure GenerateSignInCode has been called and completed successfully before attempting to sign in");
            }

            var signInRequest = new SignInWithCodeRequest() { CodeVerifier = CodeVerifier, CodeLinkSessionId = CodeLinkSessionId };
            SignInResponse signInResponse;

            try
            {
                if (usePolling)
                {
                    signInResponse = await PollForCodeConfirmationAsync(signInRequest, cancellationToken);
                }
                else
                {
                    signInResponse = await NetworkClient.SignInWithCodeAsync(signInRequest);
                    if (signInResponse.IdToken == null)
                    {
                        Logger.LogWarning("Sign In Code has not been confirmed.");
                        return;
                    }
                }
                CodeLinkSessionId = null;
                CodeVerifier = null;
                await HandleSignInRequestAsync(() => Task.FromResult(signInResponse));
            }
            catch (WebRequestException)
            {
                CodeLinkSessionId = null;
                CodeVerifier = null;
                throw ExceptionHandler.BuildUnknownException("The sign-in code was not confirmed.");
            }
        }

        async Task<SignInResponse> PollForCodeConfirmationAsync(SignInWithCodeRequest request, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await DelayWithScheduler(Settings.CodeConfirmationDelay);

                try
                {
                    var response = await NetworkClient.SignInWithCodeAsync(request);
                    if (response.IdToken != null)
                    {
                        return response;
                    }
                }
                catch (WebRequestException e)
                {
                    if (e.ResponseCode == 404)
                    {
                        SignInCodeExpired?.Invoke();
                        throw ExceptionHandler.BuildUnknownException("The sign-in code has expired.");
                    }
                }
            }

            var exception = ExceptionHandler.BuildUnknownException("The operation was canceled or timed out while waiting for code confirmation.");
            SendSignInFailedEvent(exception, true);
            throw exception;
        }

        Task DelayWithScheduler(double delaySeconds)
        {
            var tcs = new TaskCompletionSource<bool>();
            m_Scheduler.ScheduleAction(() => tcs.SetResult(true), delaySeconds);
            return tcs.Task;
        }

        public async Task<SignInCodeInfo> GetSignInCodeInfoAsync(string code)
        {
            if (IsAuthorized)
            {
                if (string.IsNullOrEmpty(code))
                {
                    throw ExceptionHandler.BuildUnknownException("Code cannot be null or empty");
                }

                try
                {
                    var request = new CodeLinkInfoRequest() { SignInCode = code };
                    var response = await NetworkClient.GetCodeIdentifierAsync(request);
                    var info = new SignInCodeInfo() { SignInCode = code, Identifier = response.Identifier, Expiration = response.Expiration };
                    return info;
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

            throw ExceptionHandler.BuildClientInvalidStateException(State);
        }

        public async Task ConfirmCodeAsync(string code, string idProvider = null, string externalToken = null)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw ExceptionHandler.BuildUnknownException("Code cannot be null or empty");
            }

            if (IsAuthorized)
            {
                try
                {
                    var sessionToken = SessionTokenExists ? SessionTokenComponent.SessionToken : null;
                    var request = new ConfirmSignInCodeRequest() { SignInCode = code, IdProvider = idProvider, SessionToken = sessionToken, ExternalToken = externalToken };
                    await NetworkClient.ConfirmCodeAsync(request);
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
    }
}
