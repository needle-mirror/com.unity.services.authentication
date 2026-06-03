using System.Threading.Tasks;

namespace Unity.Services.Authentication
{
    partial class AuthenticationServiceInternal
    {
        public async Task<RestrictedTokenResponse> GenerateRestrictedTokenAsync(RestrictedTokenOptions options)
        {
            if (!IsAuthorized)
            {
                throw ExceptionHandler.BuildClientInvalidStateException(State);
            }

            var sessionToken = SessionTokenComponent.SessionToken;
            if (string.IsNullOrEmpty(sessionToken))
            {
                throw ExceptionHandler.BuildClientSessionTokenNotExistsException();
            }

            options = options ?? new RestrictedTokenOptions();
            var request = new SessionTokenRequest
            {
                SessionToken = sessionToken,
                RetainTargetingClaims = true,
                SingleUse = options.SingleUse,
                Services = options.Services,
                TtlSeconds = options.TtlSeconds,
                Country = options.Country
            };

            try
            {
                var response = await NetworkClient.GenerateRestrictedTokenAsync(request);
                // Deliberately skip CompleteSignIn: this token is returned to the caller, not
                // applied to the current session, so the player's signed-in state stays unchanged.
                return new RestrictedTokenResponse
                {
                    UserId = response.UserId,
                    IdToken = response.IdToken,
                    SessionToken = response.SessionToken,
                    ExpiresIn = response.ExpiresIn
                };
            }
            catch (WebRequestException e)
            {
                throw ExceptionHandler.ConvertException(e);
            }
        }
    }
}
