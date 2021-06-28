using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Rsk.TokenExchange.Validators;
using Rsk.TokenExchange.Validators.Adaptors;
using Xunit;

namespace Rsk.TokenExchange.Tests.Validators
{
    public class DefaultSubjectTokenValidatorTests
    {
        private Mock<ITokenValidatorAdaptor> mockTokenValidator = new Mock<ITokenValidatorAdaptor>();
        private ILogger<ISubjectTokenValidator> logger = new NullLogger<ISubjectTokenValidator>();

        private const string validToken = "eyJ.eyJ";
        private const string validTokenType = TokenExchangeConstants.TokenTypes.AccessToken;

        public DefaultSubjectTokenValidatorTests()
        {
            mockTokenValidator.Setup(x => x.ValidateAccessToken(validToken))
                .ReturnsAsync(new TokenValidationResult(false, new List<Claim>()));
        }
        
        private DefaultSubjectTokenValidator CreateSut() 
            => new DefaultSubjectTokenValidator(mockTokenValidator?.Object, logger);

        [Fact]
        public void ctor_WhenTokenValidatorIsNull_ExpectArgumentNullException()
        {
            mockTokenValidator = null;
            Assert.Throws<ArgumentNullException>(CreateSut);
        }

        [Fact]
        public void ctor_WhenLoggerIsNull_ExpectArgumentNullException()
        {
            logger = null;
            Assert.Throws<ArgumentNullException>(CreateSut);
        }
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public Task Validate_WhenTokenIsNullOrWhitespace_ExpectArgumentNullException(string token)
            => Assert.ThrowsAsync<ArgumentNullException>(() => CreateSut().Validate(token, "jwt"));

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public Task Validate_WhenTokenTypeIsNullOrWhitespace_ExpectArgumentNullException(string tokenType)
            => Assert.ThrowsAsync<ArgumentNullException>(() => CreateSut().Validate("123", tokenType));

        [Fact]
        public async Task Validate_WhenTokenTypeIsNotAccessToken_ExpectFailureResult()
        {
            var sut = CreateSut();

            var result = await sut.Validate(validToken, TokenExchangeConstants.TokenTypes.Jwt); // JWT is just a format

            result.IsValid.Should().BeFalse();
            result.Claims.Should().BeNull();
        }
        
        [Fact]
        public async Task Validate_WhenTokenValidatorReturnsError_ExpectFailureResult()
        {
            var token = Guid.NewGuid().ToString();

            mockTokenValidator.Setup(x => x.ValidateAccessToken(token))
                .ReturnsAsync(new TokenValidationResult(true));
            var sut = CreateSut();

            var result = await sut.Validate(token, validTokenType);

            result.IsValid.Should().BeFalse();
            result.Claims.Should().BeNull();
        }
        
        [Fact]
        public async Task Validate_WhenTokenValid_ExpectSuccess()
        {
            var sut = CreateSut();

            var result = await sut.Validate(validToken, validTokenType);

            result.IsValid.Should().BeTrue();
            result.Claims.Should().NotBeNull();
        }
        
        [Fact]
        public async Task Validate_WhenTokenValid_ExpectClaimsFromTokenValidator()
        {
            var claims = new[] {new Claim("sub", "123"), new Claim("name", "alice")};

            mockTokenValidator.Setup(x => x.ValidateAccessToken(validToken))
                .ReturnsAsync(new TokenValidationResult(false, claims));
            var sut = CreateSut();

            var result = await sut.Validate(validToken, validTokenType);

            result.Claims.Should().BeSameAs(claims);
        }
    }
}