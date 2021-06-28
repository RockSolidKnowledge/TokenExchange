using System.Threading.Tasks;
using IdentityServer4.Validation;
using Rsk.TokenExchange.Validators.Adaptors;
using TokenValidationResult = Rsk.TokenExchange.Validators.Adaptors.TokenValidationResult;

namespace Rsk.TokenExchange.IdentityServer
{
    /// <summary>
    /// Wrapper around IdentityServer4's ITokenValidator
    /// </summary>
    public class IdentityServerSubjectTokenValidator : ITokenValidatorAdaptor
    {
        private readonly ITokenValidator tokenValidator;

        /// <summary>
        /// Creates a new IdentityServerSubjectTokenValidator
        /// </summary>
        /// <param name="tokenValidator"></param>
        public IdentityServerSubjectTokenValidator(ITokenValidator tokenValidator)
        {
            this.tokenValidator = tokenValidator;
        }

        /// <inheritdoc />
        public async Task<TokenValidationResult> ValidateAccessToken(string token)
        {
            var result = await tokenValidator.ValidateAccessTokenAsync(token);
            return new TokenValidationResult(result.IsError, result.Claims);
        }
    }
}