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

    /// <inheritdoc />
    public class TokenExchangeValidationResult : ITokenExchangeValidationResult
    {
        /// <inheritdoc />
        public bool IsValid { get; private set; }
        
        /// <inheritdoc />
        public string ErrorDescription { get; private set; }
        
        /// <inheritdoc />
        public IEnumerable<Claim> Claims { get; private set; }

        private TokenExchangeValidationResult() { }

        /// <summary>
        /// Successful token exchange validation.
        /// </summary>
        /// <param name="claims">The claims extracted from the token exchange request. Optional.</param>
        public static TokenExchangeValidationResult Success(IEnumerable<Claim> claims = null)
        {
            return new TokenExchangeValidationResult
            {
                IsValid = true,
                Claims = claims ?? new List<Claim>()
            };
        }

        /// <summary>
        /// Failed token exchange validation.
        /// </summary>
        /// <param name="errorDescription">An OAuth-level error description (error_description). Optional.</param>
        /// <returns></returns>
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