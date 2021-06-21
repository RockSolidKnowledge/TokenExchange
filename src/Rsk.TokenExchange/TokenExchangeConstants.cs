namespace Rsk.TokenExchange
{
    public class TokenExchangeConstants
    {
        public const string GrantType = "urn:ietf:params:oauth:grant-type:token-exchange";

        // https://www.rfc-editor.org/rfc/rfc8693.html#name-request
        public class RequestParameters
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

        public class ResponseParameters
        {
            public const string IssuedTokenType = "issued_token_type";
        }
        
        // https://www.rfc-editor.org/rfc/rfc8693.html#name-token-type-identifiers
        public class TokenTypes
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