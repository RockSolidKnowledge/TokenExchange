using System;
using System.Threading.Tasks;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using Rsk.TokenExchange.Validators;

namespace Rsk.TokenExchange.IdentityServer
{
    /// <summary>
    /// Wrapper around IdentityServer's ITokenValidator
    /// Returns success for any token issued by IdentityServer.
    /// </summary>
    public class IdentityServerSubjectTokenValidator : ISubjectTokenValidator
    {
        private readonly ITokenValidator tokenValidator;
        private readonly ILogger<ISubjectTokenValidator> logger;

        /// <summary>
        /// Creates a new IdentityServerSubjectTokenValidator
        /// </summary>
        public IdentityServerSubjectTokenValidator(ITokenValidator tokenValidator, ILogger<ISubjectTokenValidator> logger)
        {
            this.tokenValidator = tokenValidator ?? throw new ArgumentNullException(nameof(tokenValidator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<ISubjectTokenValidationResult> Validate(string token, string tokenType)
        {
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrWhiteSpace(tokenType)) throw new ArgumentNullException(nameof(tokenType));

            if (tokenType != TokenExchangeConstants.TokenTypes.AccessToken)
            {
                logger.LogError($"Received unsupported token type of {tokenType}");
                return SubjectTokenValidationResult.Failure();
            }
            
            var result = await tokenValidator.ValidateAccessTokenAsync(token);
            if (result.IsError)
            {
                logger.LogError($"Received invalid token");
                return SubjectTokenValidationResult.Failure();
            }
            
            return SubjectTokenValidationResult.Success(result.Claims);
        }
    }
}