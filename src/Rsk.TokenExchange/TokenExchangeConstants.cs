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
            public const string GrantType = "grant_type";
            public const string Resource = "resource";
            public const string Audience = "audience";
            public const string Scope = "scope";
            public const string RequestedTokenType = "requested_token_type";
            public const string SubjectToken = "subject_token";
            public const string SubjectTokenType = "subject_token_type";
            public const string ActorToken = "actor_token";
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
            public const string AccessToken = "urn:ietf:params:oauth:token-type:access_token";
            public const string RefreshToken = "urn:ietf:params:oauth:token-type:refresh_token";
            public const string IdentityToken = "urn:ietf:params:oauth:token-type:id_token";
            public const string Saml1Assertion = "urn:ietf:params:oauth:token-type:saml1";
            public const string Saml2Assertion = "urn:ietf:params:oauth:token-type:saml2";
            public const string Jwt = "urn:ietf:params:oauth:token-type:jwt";
        }
    }
}