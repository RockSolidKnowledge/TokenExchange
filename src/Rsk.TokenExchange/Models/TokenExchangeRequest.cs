using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Rsk.TokenExchange.Exceptions;

namespace Rsk.TokenExchange
{
    /// <inheritdoc />
    public class TokenExchangeRequest : ITokenExchangeRequest
    {
        /// <summary>
        /// Parses a token exchange token exchange request from a NameValueCollection. 
        /// </summary>
        /// <param name="clientId">The ID of the client (client_id) that made the token exchange request.</param>
        /// <param name="request">The NameValueCollection containing the request parameters</param>
        /// <exception cref="InvalidRequestException">Throw on missing grant_type, subject_token, and subject_token_type.</exception>
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

        /// <inheritdoc />
        public string ClientId { get; }
        
        /// <inheritdoc />
        public string GrantType { get; }
        
        /// <inheritdoc />
        public string Resource { get; }
        
        /// <inheritdoc />
        public string Audience { get; }
        
        /// <inheritdoc />
        public IEnumerable<string> Scope { get; }
        
        /// <inheritdoc />
        public string RequestedTokenType { get; }
        
        /// <inheritdoc />
        public string SubjectToken { get; }
        
        /// <inheritdoc />
        public string SubjectTokenType { get; }
        
        /// <inheritdoc />
        public string ActorToken { get; }
        
        /// <inheritdoc />
        public string ActorTokenType { get; }
    }

    /// <summary>
    /// Represents the token exchange request.
    /// </summary>
    public interface ITokenExchangeRequest
    {
        /// <summary>
        /// REQUIRED.
        /// The identifier of the client application.
        /// </summary>
        string ClientId { get; }
        
        /// <summary>
        /// REQUIRED.
        /// The OAuth grant type.
        /// Must be "urn:ietf:params:oauth:grant-type:token-exchange".
        /// </summary>
        string GrantType { get; }
        
        /// <summary>
        /// OPTIONAL.
        /// A URI indicating the resource where the requester intends to use the token.
        /// </summary>
        string Resource { get; }
        
        /// <summary>
        /// OPTIONAL.
        /// The name of the resource where the requester intends to use the token.
        /// Maps to an ApiResource in IdentityServer.
        /// </summary>
        string Audience { get; }
        
        /// <summary>
        /// OPTIONAL.
        /// The scope of the requested token.
        /// Maps to ApiScopes in IdentityServer.
        /// </summary>
        IEnumerable<string> Scope { get; }
        
        /// <summary>
        /// OPTIONAL.
        /// The type of the requested token.
        /// RFC 8693 defines some known <see cref="TokenExchangeConstants.TokenTypes"/>.
        /// </summary>
        string RequestedTokenType { get; }
        
        /// <summary>
        /// REQUIRED.
        /// The token representing the identity of the party the request is being made on behalf of.
        /// Typically, the subject of this token will be the subject of the token issued in response to the request.
        /// </summary>
        string SubjectToken { get; }
        
        /// <summary>
        /// REQUIRED.
        /// The type of the subject token.
        /// RFC 8693 defines some known <see cref="TokenExchangeConstants.TokenTypes"/>.
        /// </summary>
        string SubjectTokenType { get; }
        
        /// <summary>
        /// OPTIONAL.
        /// The token that represents the identity of the acting party.
        /// Typically, this will be the party that is authorized to use the requested token and act on behalf of the subject.
        /// </summary>
        string ActorToken { get; }
        
        /// <summary>
        /// REQUIRED when actor token is present.
        /// The type of the actor token.
        /// RFC 8693 defines some known <see cref="TokenExchangeConstants.TokenTypes"/>.
        /// </summary>
        string ActorTokenType { get; }
    }
}