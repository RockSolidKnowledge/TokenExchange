using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using Rsk.TokenExchange.Exceptions;
using Rsk.TokenExchange.Validators;

namespace Rsk.TokenExchange.IdentityServer
{
    public class TokenExchangeExtensionGrantValidator : IExtensionGrantValidator
    {
        private readonly ITokenExchangeRequestParser parser;
        private readonly ITokenExchangeRequestValidator requestValidator;
        private readonly ILogger<TokenExchangeExtensionGrantValidator> logger;

        public TokenExchangeExtensionGrantValidator(
            ITokenExchangeRequestParser parser,
            ITokenExchangeRequestValidator requestValidator,
            ILogger<TokenExchangeExtensionGrantValidator> logger)
        {
            this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
            this.requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public string GrantType => TokenExchangeConstants.GrantType;
        
        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            // warn on no client auth
            if (context.Request.Secret == null) logger.LogInformation("Recieved unauthenticated token exchange request");

            // parse request
            ITokenExchangeRequest tokenExchangeRequest;
            try
            {
                tokenExchangeRequest = parser.Parse(context.Request.ClientId, context.Request.Raw);
            }
            catch (TokenExchangeException e)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, e.Message);
                return;
            }
            
            // validate request
            var validationResult = await requestValidator.Validate(tokenExchangeRequest);
            if (!validationResult.IsValid)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid subject token");
                return;
            }

            // parse subject claims
            var sub = validationResult.Claims.FirstOrDefault(x => x.Type == "sub")?.Value
                      ?? context.Request.ClientId;

            context.Result = new GrantValidationResult(
                subject: sub,
                authenticationMethod: GrantType,
                claims: validationResult.Claims,
                customResponse: CustomResponseParameters);
        }

        private static readonly Dictionary<string, object> CustomResponseParameters = new Dictionary<string, object>
            {{TokenExchangeConstants.ResponseParameters.IssuedTokenType, TokenExchangeConstants.TokenTypes.AccessToken}};
    }
}