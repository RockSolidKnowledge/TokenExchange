using System.Collections.Generic;
using System.Security.Claims;

namespace Rsk.TokenExchange.Validators
{
    /// <summary>
    /// Result of the token exchange request validation.
    /// </summary>
    public interface ITokenExchangeValidationResult
    {
        /// <summary>
        /// If the request passed validation.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// The error description to return in the token response.
        /// </summary>
        string ErrorDescription { get; }
        
        /// <summary>
        /// The custom claims parsed from the token request.
        /// </summary>
        IEnumerable<Claim> Claims { get; }
    }

    public class TokenExchangeValidationResult : ITokenExchangeValidationResult
    {
        public bool IsValid { get; private set; }
        public string ErrorDescription { get; private set; }
        public IEnumerable<Claim> Claims { get; private set; }

        private TokenExchangeValidationResult() { }

        public static TokenExchangeValidationResult Success(IEnumerable<Claim> claims = null)
        {
            return new TokenExchangeValidationResult
            {
                IsValid = true,
                Claims = claims ?? new List<Claim>()
            };
        }

        public static TokenExchangeValidationResult Failure(string errorDescription = null)
        {
            return new TokenExchangeValidationResult
            {
                IsValid = false,
                ErrorDescription = errorDescription
            };
        }
    }
}