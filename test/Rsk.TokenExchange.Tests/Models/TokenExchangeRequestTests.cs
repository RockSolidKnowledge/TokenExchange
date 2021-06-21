using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using FluentAssertions;
using Rsk.TokenExchange.Exceptions;
using Xunit;

namespace Rsk.TokenExchange.Tests.Models
{
    public class TokenExchangeRequestTests
    {
        private readonly NameValueCollection rawRequest = new NameValueCollection
        {
            {TokenExchangeConstants.RequestParameters.GrantType, TokenExchangeConstants.GrantType},
            {TokenExchangeConstants.RequestParameters.SubjectToken, "eyJ.eyJ"},
            {TokenExchangeConstants.RequestParameters.SubjectTokenType, TokenExchangeConstants.TokenTypes.AccessToken}
        };
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ctor_WhenClientIdIsNullOrWhitespace_ExpectArgumentNullException(string clientId) 
            => Assert.Throws<ArgumentNullException>(() => new TokenExchangeRequest(clientId, rawRequest));
        
        [Fact]
        public void ctor_WhenNameValueCollectionIsNull_ExpectArgumentNullException() 
            => Assert.Throws<ArgumentNullException>(() => new TokenExchangeRequest("123", null));

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ctor_WhenGrantTypeIsNullOrWhitespace_ExpectInvalidRequestException(string grantType)
        {
            rawRequest[TokenExchangeConstants.RequestParameters.GrantType] = grantType;
            Assert.Throws<InvalidRequestException>(() => new TokenExchangeRequest("123", rawRequest));
        }

        [Fact]
        public void ctor_WhenGrantTypeIsNotTokenExchangeType_ExpectInvalidRequestException()
        {
            rawRequest[TokenExchangeConstants.RequestParameters.GrantType] = "code";
            Assert.Throws<InvalidRequestException>(() => new TokenExchangeRequest("123", rawRequest));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ctor_WhenSubjectTokenIsNullOrWhitespace_ExpectInvalidRequestException(string subjectToken)
        {
            rawRequest[TokenExchangeConstants.RequestParameters.SubjectToken] = subjectToken;
            Assert.ThrowsAny<InvalidRequestException>(() => new TokenExchangeRequest("123", rawRequest));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ctor_WhenSubjectTokenTypeIsNullOrWhitespace_ExpectInvalidRequestException(string subjectTokenType)
        {
            rawRequest[TokenExchangeConstants.RequestParameters.SubjectTokenType] = subjectTokenType;
            Assert.ThrowsAny<InvalidRequestException>(() => new TokenExchangeRequest("123", rawRequest));
        }

        [Fact]
        public void ctor_WhenActorTokenPresentButActorTokenTypeIsMissing_ExpectInvalidRequestException()
        {
            rawRequest[TokenExchangeConstants.RequestParameters.ActorToken] = "eyJ.eyJ";
            rawRequest[TokenExchangeConstants.RequestParameters.ActorTokenType] = null;
            
            Assert.ThrowsAny<InvalidRequestException>(() => new TokenExchangeRequest("123", rawRequest));
        }

        [Fact]
        public void ctor_WhenScopesNotInRequest_ExpectScopesSetToNull()
        {
            rawRequest[TokenExchangeConstants.RequestParameters.Scope] = null;
            
            var request = new TokenExchangeRequest("123", rawRequest);
            
            request.Scope.Should().BeNull();
        }

        [Fact]
        public void ctor_WhenAllParametersPresent_ExpectCorrectPropertyValues()
        {
            const string clientId = "123";
            const string grantType = TokenExchangeConstants.GrantType;
            const string resource = "urn:api1";
            const string audience = "api1";
            IEnumerable<string> scopes = new[] {"api1.read", "api1.write"};
            const string requestedTokenType = TokenExchangeConstants.TokenTypes.Jwt;
            const string subjectToken = "4171466da8f841b6913ad723ed598450";
            const string subjectTokenType = TokenExchangeConstants.TokenTypes.AccessToken;
            const string actorToken = "0c3685db9f3548b1b9340322b962db5d";
            const string actorTokenType = TokenExchangeConstants.TokenTypes.RefreshToken;

            var request = new TokenExchangeRequest("123", new NameValueCollection
            {
                {TokenExchangeConstants.RequestParameters.GrantType, grantType},
                {TokenExchangeConstants.RequestParameters.Resource, resource},
                {TokenExchangeConstants.RequestParameters.Audience, audience},
                {TokenExchangeConstants.RequestParameters.Scope, scopes.Aggregate((x, y) => x + " " + y)},
                {TokenExchangeConstants.RequestParameters.RequestedTokenType, requestedTokenType},
                {TokenExchangeConstants.RequestParameters.SubjectToken, subjectToken},
                {TokenExchangeConstants.RequestParameters.SubjectTokenType, subjectTokenType},
                {TokenExchangeConstants.RequestParameters.ActorToken, actorToken},
                {TokenExchangeConstants.RequestParameters.ActorTokenType, actorTokenType}
            });

            request.ClientId.Should().Be(clientId);
            request.GrantType.Should().Be(grantType);
            request.Resource.Should().Be(resource);
            request.Audience.Should().Be(audience);
            request.Scope.Should().BeEquivalentTo(scopes);
            request.RequestedTokenType.Should().Be(requestedTokenType);
            request.SubjectToken.Should().Be(subjectToken);
            request.SubjectTokenType.Should().Be(subjectTokenType);
            request.ActorToken.Should().Be(actorToken);
            request.ActorTokenType.Should().Be(actorTokenType);
        }
    }
}