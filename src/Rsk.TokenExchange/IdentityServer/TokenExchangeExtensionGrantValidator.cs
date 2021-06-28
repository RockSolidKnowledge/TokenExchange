using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using Rsk.TokenExchange.Exceptions;
using Rsk.TokenExchange.Validators;

namespace Rsk.TokenExchange.IdentityServer
{
    /// <summary>
    /// Extension grant validator for OAuth Token Exchange (RFC 8693).
    /// </summary>
    public class TokenExchangeExtensionGrantValidator : IExtensionGrantValidator
    {
        private readonly ITokenExchangeRequestParser parser;
        private readonly ITokenExchangeRequestValidator requestValidator;
        private readonly ITokenExchangeClaimsParser claimsParser;
        private readonly ILogger<TokenExchangeExtensionGrantValidator> logger;

        public TokenExchangeExtensionGrantValidator(
            ITokenExchangeRequestParser parser,
            ITokenExchangeRequestValidator requestValidator,
            ITokenExchangeClaimsParser claimsParser,
            ILogger<TokenExchangeExtensionGrantValidator> logger)
        {
            this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
            this.requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
            this.claimsParser = claimsParser ?? throw new ArgumentNullException(nameof(claimsParser));
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

            // parse claims
            string subject;
            IEnumerable<Claim> claims;
            try
            {
                subject = await claimsParser.ParseSubject(validationResult.Claims, tokenExchangeRequest);
                claims = await claimsParser.ParseClaims(validationResult.Claims, tokenExchangeRequest);
            }
            catch (TokenExchangeException e)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, $"Unable to generate claims. {e.GetType()} - {e.Message}");
                return;
            }

            if (subject == null)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Unable to parse subject claim - IdentityServer requires subject claim for extension grants");
                return;
            }
            
            context.Result = new GrantValidationResult(
                subject: subject,
                authenticationMethod: GrantType,
                claims: claims,
                customResponse: CustomResponseParameters);
        }
        
        private static readonly Dictionary<string, object> CustomResponseParameters = new Dictionary<string, object>
            {{TokenExchangeConstants.ResponseParameters.IssuedTokenType, TokenExchangeConstants.TokenTypes.AccessToken}};
    }
}