using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using IdentityModel;
using Microsoft.Extensions.Logging;
using Rsk.TokenExchange.Exceptions;
using Rsk.TokenExchange.Validators;

namespace Rsk.TokenExchange.DuendeIdentityServer
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

        /// <summary>
        /// Creates a new <see cref="TokenExchangeExtensionGrantValidator"/>.
        /// </summary>
        /// <param name="parser">Parser for understanding token exchange requests.</param>
        /// <param name="requestValidator">Validator for the token exchange request.</param>
        /// <param name="claimsParser">Parser for generating the subject claim and additional claims for the subject.</param>
        /// <param name="logger">The logger.</param>
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

        /// <inheritdoc />
        /// <value>urn:ietf:params:oauth:grant-type:token-exchange</value>
        public string GrantType => TokenExchangeConstants.GrantType;

        /// <inheritdoc />
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
            IList<Claim> claims;
            try
            {
                subject = await claimsParser.ParseSubject(validationResult.Claims, tokenExchangeRequest);
                claims = (await claimsParser.ParseClaims(validationResult.Claims, tokenExchangeRequest)).ToList();
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

            UpdateRequest(context.Request, claims);
            
            context.Result = new GrantValidationResult(
                subject: subject,
                authenticationMethod: GrantType,
                claims: claims,
                customResponse: CustomResponseParameters);
        }
        
        /// <summary>
        /// Update original token request to enable token exchange
        /// </summary>
        public virtual void UpdateRequest(ValidatedTokenRequest request, IEnumerable<Claim> claims)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (claims == null) throw new ArgumentNullException(nameof(claims));

            // update client_id to act as original client app
            var clientId = claims.First(x => x.Type == JwtClaimTypes.ClientId).Value;
            request.ClientId = clientId;
        }
        
        private static readonly Dictionary<string, object> CustomResponseParameters = new Dictionary<string, object>
            {{TokenExchangeConstants.ResponseParameters.IssuedTokenType, TokenExchangeConstants.TokenTypes.AccessToken}};
    }
}