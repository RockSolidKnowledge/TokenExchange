namespace Rsk.TokenExchange
{
    /// <summary>
    /// Constants for OAuth Token Exchange (RFC 8693).
    /// </summary>
    public static class TokenExchangeConstants
    {
        /// <summary>
        /// grant_type reserved for OAuth Token Exchange.
        /// </summary>
        public const string GrantType = "urn:ietf:params:oauth:grant-type:token-exchange";

        /// <summary>
        /// Token request parameters for OAuth Token Exchange.
        /// https://www.rfc-editor.org/rfc/rfc8693.html#name-request.
        /// </summary>
        public static class RequestParameters
        {
            
            /// <inheritdoc cref="ITokenExchangeRequest.GrantType"/>
            public const string GrantType = "grant_type";
            
            /// <inheritdoc cref="ITokenExchangeRequest.Resource"/>
            public const string Resource = "resource";
            
            /// <inheritdoc cref="ITokenExchangeRequest.Audience"/>
            public const string Audience = "audience";
            
            /// <inheritdoc cref="ITokenExchangeRequest.Scope"/>
            public const string Scope = "scope";
            
            /// <inheritdoc cref="ITokenExchangeRequest.RequestedTokenType"/>
            public const string RequestedTokenType = "requested_token_type";
            
            /// <inheritdoc cref="ITokenExchangeRequest.SubjectToken"/>
            public const string SubjectToken = "subject_token";
            
            /// <inheritdoc cref="ITokenExchangeRequest.SubjectTokenType"/>
            public const string SubjectTokenType = "subject_token_type";
            
            /// <inheritdoc cref="ITokenExchangeRequest.ActorToken"/>
            public const string ActorToken = "actor_token";
            
            /// <inheritdoc cref="ITokenExchangeRequest.ActorTokenType"/>
            public const string ActorTokenType = "actor_token_type";
        }

        /// <summary>
        /// Custom token response parameters for OAuth Token Exchange.
        /// https://www.rfc-editor.org/rfc/rfc8693.html#name-successful-response.
        /// </summary>
        public static class ResponseParameters
        {
            /// <summary>
            /// REQUIRED. An identifier for the representation of the issued security token.
            /// Should use a value from <see cref="TokenTypes"/>.
            /// </summary>
            public const string IssuedTokenType = "issued_token_type";
        }
        
        /// <summary>
        /// The type of token being exchanged or generated as the result of exchange.
        /// https://www.rfc-editor.org/rfc/rfc8693.html#name-token-type-identifiers.
        /// </summary>
        public static class TokenTypes
        {
            /// <summary>
            /// Indicates that the token is an OAuth 2.0 access token issued by the given authorization server.
            /// </summary>
            public const string AccessToken = "urn:ietf:params:oauth:token-type:access_token";
            
            /// <summary>
            /// Indicates that the token is an OAuth 2.0 refresh token issued by the given authorization server.
            /// </summary>
            public const string RefreshToken = "urn:ietf:params:oauth:token-type:refresh_token";
            
            /// <summary>
            /// Indicates that the token is an ID Token as defined in OpenID Connect.
            /// </summary>
            public const string IdentityToken = "urn:ietf:params:oauth:token-type:id_token";
            
            /// <summary>
            /// Indicates that the token is a base64url-encoded SAML 1.1 assertion.
            /// </summary>
            public const string Saml1Assertion = "urn:ietf:params:oauth:token-type:saml1";
            
            /// <summary>
            /// Indicates that the token is a base64url-encoded SAML 2.0 assertion.
            /// </summary>
            public const string Saml2Assertion = "urn:ietf:params:oauth:token-type:saml2";
            
            /// <summary>
            /// Indicates that the token is a JWT (and not an access token).
            /// For example, a JWT used as an authorization grant. 
            /// </summary>
            public const string Jwt = "urn:ietf:params:oauth:token-type:jwt";
        }
    }
}