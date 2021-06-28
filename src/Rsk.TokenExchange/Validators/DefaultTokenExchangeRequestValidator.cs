using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rsk.TokenExchange.Validators
{
    /// <summary>
    /// Default implementation of ITokenExchangeRequestValidator.
    /// Validates the subject token, expecting an access token, issued by this authorization server, that has an audience or client_id of the requesting client.
    /// </summary>
    public class DefaultTokenExchangeRequestValidator : ITokenExchangeRequestValidator
    {
        private readonly ISubjectTokenValidator subjectTokenValidator;

        /// <summary>
        /// Creates a new DefaultTokenExchangeRequestValidator.
        /// </summary>
        public DefaultTokenExchangeRequestValidator(ISubjectTokenValidator subjectTokenValidator)
        {
            this.subjectTokenValidator = subjectTokenValidator ?? throw new ArgumentNullException(nameof(subjectTokenValidator));
        }

        /// <inheritdoc />
        public async Task<ITokenExchangeValidationResult> Validate(ITokenExchangeRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // validate subject token is issued by IdentityServer
            var result = await subjectTokenValidator.Validate(request.SubjectToken, request.SubjectTokenType);

            // validate that the requester is an intended audience of the subject token
            var audiences = result.Claims.Where(x => x.Type == "aud").ToList();
            var clientId = result.Claims.FirstOrDefault(x => x.Type == "client_id");
            if (audiences.All(x => x.Value != request.ClientId) && clientId?.Value != request.ClientId)
            {
                return TokenExchangeValidationResult.Failure("Requester must be a recipient of the subject token");
            }

            return result.IsValid 
                ? TokenExchangeValidationResult.Success(result.Claims)
                : TokenExchangeValidationResult.Failure("Invalid subject token");
        }
    }
}