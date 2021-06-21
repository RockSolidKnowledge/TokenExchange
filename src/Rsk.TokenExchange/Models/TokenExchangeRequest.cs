using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Rsk.TokenExchange.Exceptions;

namespace Rsk.TokenExchange
{
    public class TokenExchangeRequest : ITokenExchangeRequest
    {
        public TokenExchangeRequest(string clientId, NameValueCollection request)
        {
            if (string.IsNullOrWhiteSpace(clientId)) throw new ArgumentNullException(nameof(clientId));
            if (request == null) throw new ArgumentNullException(nameof(request));
            
            ClientId = clientId;

            // grant type must be urn:ietf:params:oauth:grant-type:token-exchange
            GrantType = request[TokenExchangeConstants.RequestParameters.GrantType];
            if (GrantType != TokenExchangeConstants.GrantType)
                throw new InvalidRequestException($"Token exchange request must have grant type of {TokenExchangeConstants.GrantType}");

            // subject token and subject token type must be present
            SubjectToken = request[TokenExchangeConstants.RequestParameters.SubjectToken];
            if (string.IsNullOrWhiteSpace(SubjectToken)) throw new InvalidRequestException("Token exchange request must contain subject token");
            
            SubjectTokenType = request[TokenExchangeConstants.RequestParameters.SubjectTokenType];
            if (string.IsNullOrWhiteSpace(SubjectTokenType)) throw new InvalidRequestException("Token exchange request must contain subject token type");

            ActorToken = request[TokenExchangeConstants.RequestParameters.ActorToken];
            ActorTokenType = request[TokenExchangeConstants.RequestParameters.ActorTokenType];

            // if actor token is present, so must the actor token type. Actor token type must not be present if there is no actor token
            if ((ActorToken != null && ActorTokenType == null) || (ActorToken == null && ActorTokenType != null))
                throw new InvalidRequestException();
            
            Resource = request[TokenExchangeConstants.RequestParameters.Resource];
            Audience = request[TokenExchangeConstants.RequestParameters.Audience];
            Scope = request[TokenExchangeConstants.RequestParameters.Scope]?.Split(' ').ToList();

            RequestedTokenType = request[TokenExchangeConstants.RequestParameters.RequestedTokenType];
        }

        public string ClientId { get; set; }
        public string GrantType { get; }
        
        public string Resource { get; }
        public string Audience { get; }
        public IEnumerable<string> Scope { get; }
        
        public string RequestedTokenType { get; }
        
        public string SubjectToken { get; }
        public string SubjectTokenType { get; }
        
        public string ActorToken { get; }
        public string ActorTokenType { get; }
    }

    public interface ITokenExchangeRequest
    {
        /// <summary>
        /// REQUIRED.
        /// The identifier of the client application.
        /// </summary>
        string ClientId { get; }
        
        /// <summary>
        /// REQUIRED.
        /// Must be "urn:ietf:params:oauth:grant-type:token-exchange".
        /// </summary>
        string GrantType { get; }
        
        /// <summary>
        /// OPTIONAL.
        /// A URI indicating the resource where the requester intends to use the token.
        /// Unsupported.
        /// </summary>
        string Resource { get; }
        
        /// <summary>
        /// OPTIONAL.
        /// The name of the resource where the requester intends to use the token.
        /// Maps to an ApiResource in IdentityServer4.
        /// </summary>
        string Audience { get; }
        
        /// <summary>
        /// OPTIONAL.
        /// The scope of the requested token.
        /// Maps to ApiScopes in IdentityServer4.
        /// </summary>
        IEnumerable<string> Scope { get; }
        
        /// <summary>
        /// OPTIONAL.
        /// The type of the requested token, as described in rfc 8693 section 3.
        /// </summary>
        string RequestedTokenType { get; }
        
        /// <summary>
        /// REQUIRED.
        /// The token representing the identity of the requester.
        /// </summary>
        string SubjectToken { get; }
        
        /// <summary>
        /// REQUIRED.
        /// The type of the subject token, as described in rfc 8693 section 3.
        /// </summary>
        string SubjectTokenType { get; }
        
        /// <summary>
        /// OPTIONAL.
        /// The token representing the identity of the party the request is being made on behalf of.
        /// </summary>
        string ActorToken { get; }
        
        /// <summary>
        /// REQUIRED when actor token is present.
        /// The type of the actor token, as described in rfc 8693 section 3.
        /// </summary>
        string ActorTokenType { get; }
    }
}