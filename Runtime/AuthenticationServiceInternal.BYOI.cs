using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Services.Core;

namespace Unity.Services.Authentication
{
    partial class AuthenticationServiceInternal
    {
        const string k_IdProviderNameRegex = @"^oidc-[a-z0-9-_\.]{1,15}$";

        public Task SignInWithOpenIdConnectAsync(string idProviderName, string idToken, SignInOptions options = null)
        {
            if (!ValidateOpenIdConnectIdProviderName(idProviderName))
            {
                throw ExceptionHandler.BuildInvalidIdProviderNameException();
            }
            return SignInWithExternalTokenAsync(idProviderName, new SignInWithExternalTokenRequest
            {
                IdProvider = idProviderName,
                Token = idToken,
                SignInOnly = !options?.CreateAccount ?? false
            });
        }

        public Task LinkWithOpenIdConnectAsync(string idProviderName, string idToken, LinkOptions options = null)
        {
            if (!ValidateOpenIdConnectIdProviderName(idProviderName))
            {
                throw ExceptionHandler.BuildInvalidIdProviderNameException();
            }
            return LinkWithExternalTokenAsync(idProviderName, new LinkWithExternalTokenRequest()
            {
                IdProvider = idProviderName,
                Token = idToken,
                ForceLink = options?.ForceLink ?? false
            });
        }

        public Task UnlinkOpenIdConnectAsync(string idProviderName)
        {
            if (!ValidateOpenIdConnectIdProviderName(idProviderName))
            {
                throw ExceptionHandler.BuildInvalidIdProviderNameException();
            }
            return UnlinkExternalTokenAsync(idProviderName);
        }

        public void ProcessAuthenticationTokens(string accessToken, string sessionToken = null)
        {
            if (State == AuthenticationState.SignedOut || State == AuthenticationState.Expired)
            {
                try
                {
                    ValidateAccessToken(accessToken);
                }
                catch (RequestFailedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw AuthenticationException.Create(CommonErrorCodes.Unknown, $"Failed validating access token: {e.Message}");
                }

                CompleteSignIn(accessToken, sessionToken);
                return;
            }

            var exception = ExceptionHandler.BuildClientInvalidStateException(State);
            throw exception;
        }

        void ValidateAccessToken(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw AuthenticationException.Create(CommonErrorCodes.InvalidToken, "Empty or null access token.");
            }
            var accessTokenDecoded = m_JwtDecoder.Decode<AccessToken>(accessToken);
            if (accessTokenDecoded == null)
            {
                throw AuthenticationException.Create(CommonErrorCodes.InvalidToken, "Failed to decode and verify access token.");
            }
            var envName = accessTokenDecoded.Audience.FirstOrDefault(s => s.StartsWith("envName:"))?.Replace("envName:", "");
            if (EnvironmentComponent.Current != envName)
            {
                throw AuthenticationException.Create(AuthenticationErrorCodes.EnvironmentMismatch, $"The configured environment({EnvironmentComponent.Current}) and the access token one({envName ?? "null"}) don't match.");
            }
        }

        bool ValidateOpenIdConnectIdProviderName(string idProviderName)
        {
            return !string.IsNullOrEmpty(idProviderName) && Regex.Match(idProviderName, k_IdProviderNameRegex).Success;
        }
    }
}
