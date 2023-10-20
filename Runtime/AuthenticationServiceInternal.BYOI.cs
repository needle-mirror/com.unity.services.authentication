using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

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
                CompleteSignIn(accessToken, sessionToken);
                return;
            }

            var exception = ExceptionHandler.BuildClientInvalidStateException(State);
            throw exception;
        }

        bool ValidateOpenIdConnectIdProviderName(string idProviderName)
        {
            return !string.IsNullOrEmpty(idProviderName) && Regex.Match(idProviderName, k_IdProviderNameRegex).Success;
        }
    }
}
