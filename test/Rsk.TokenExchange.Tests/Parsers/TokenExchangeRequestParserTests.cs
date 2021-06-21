using System;
using System.Collections.Specialized;
using FluentAssertions;
using Rsk.TokenExchange.Exceptions;
using Xunit;

namespace Rsk.TokenExchange.Tests.Parsers
{
    public class TokenExchangeRequestParserTests
    {
        private readonly TokenExchangeRequestParser sut = new TokenExchangeRequestParser();

        private readonly NameValueCollection parameters = new NameValueCollection
        {
            {TokenExchangeConstants.RequestParameters.GrantType, TokenExchangeConstants.GrantType},
            {TokenExchangeConstants.RequestParameters.SubjectToken, "eyJ.eyJ"},
            {TokenExchangeConstants.RequestParameters.SubjectTokenType, TokenExchangeConstants.TokenTypes.AccessToken}
        };
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Parse_WhenClientIdIsNullOrWhitespace_ExpectArgumentNullException(string clientId) 
            => Assert.Throws<ArgumentNullException>(() => sut.Parse(clientId, parameters));

        [Fact]
        public void Parse_WhenParametersAreNull_ExpectArgumentNullException()
            => Assert.Throws<ArgumentNullException>(() => sut.Parse("123", null));

        [Fact]
        public void Parse_WhenInvalidTokenExchangeRequest_ExpectTokenExchangeException()
        {
            var parameters = new NameValueCollection {{"grant_type", "client_credentials"}};

            Assert.Throws<InvalidRequestException>(() => sut.Parse("123", parameters));
        }

        [Fact]
        public void Parse_WhenValidTokenExchangeRequest_ExpectParsedTokenExchangeRequest()
        {
            var parsedRequest = sut.Parse("123", parameters);

            parsedRequest.Should().BeOfType<TokenExchangeRequest>();
        }
    }
}