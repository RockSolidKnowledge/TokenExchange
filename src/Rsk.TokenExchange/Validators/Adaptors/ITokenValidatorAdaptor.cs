using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rsk.TokenExchange.Validators.Adaptors
{
    /// <summary>
    /// Adaptor for IdentityServer's ITokenValidator.
    /// </summary>
    public interface ITokenValidatorAdaptor
    {
        /// <summary>
        /// Calls into IdentityServer's ITokenValidator.ValidateAccessTokenAsync.
        /// </summary>
        Task<TokenValidationResult> ValidateAccessToken(string token);
    }

    /// <summary>
    /// The token validation result from IdentityServer's ITokenValidator.
    /// </summary>
    public class TokenValidationResult
    {
        /// <summary>
        /// If the token validation returned an error.
        /// </summary>
        public bool IsError { get; }
        
        /// <summary>
        /// The claims parsed from the token.
        /// </summary>
        public IEnumerable<Claim> Claims { get; }
        
        /// <summary>
        /// Creates a new TokenValidationResult.
        /// </summary>
        public TokenValidationResult(bool isError, IEnumerable<Claim> claims = null)
        {
            IsError = isError;
            Claims = claims;
        }
    }
}