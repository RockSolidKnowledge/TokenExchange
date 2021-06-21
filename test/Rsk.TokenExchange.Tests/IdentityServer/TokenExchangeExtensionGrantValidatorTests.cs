using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Rsk.TokenExchange.Exceptions;
using Rsk.TokenExchange.IdentityServer;
using Rsk.TokenExchange.Validators;
using Xunit;

namespace Rsk.TokenExchange.Tests.IdentityServer
{
    public class TokenExchangeExtensionGrantValidatorTests
    {
        private Mock<ITokenExchangeRequestParser> mockParser = new Mock<ITokenExchangeRequestParser>();
        private Mock<ITokenExchangeRequestValidator> mockRequestValidator = new Mock<ITokenExchangeRequestValidator>();
        private ILogger<TokenExchangeExtensionGrantValidator> logger = new NullLogger<TokenExchangeExtensionGrantValidator>();

        private TokenExchangeExtensionGrantValidator CreateSut()
            => new TokenExchangeExtensionGrantValidator(mockParser?.Object, mockRequestValidator?.Object, logger);

        [Fact]
        public void ctor_WhenParserIsNull_ExpectArgumentNullException()
        {
            mockParser = null;
            Assert.Throws<ArgumentNullException>(CreateSut);
        }

        [Fact]
        public void ctor_WhenTokenValidatorIsNull_ExpectArgumentNullException()
        {
            mockRequestValidator = null;
            Assert.Throws<ArgumentNullException>(CreateSut);
        }
        
        [Fact]
        public void ctor_WhenLoggerIsNull_ExpectArgumentNullException()
        {
            logger = null;
            Assert.Throws<ArgumentNullException>(CreateSut);
        }
        
        [Fact]
        public void GrantType_ExpectRfcGrantType()
        {
            var sut = CreateSut();
            sut.GrantType.Should().Be(TokenExchangeConstants.GrantType);
        }

        [Fact]
        public void ValidateAsync_WhenContextIsNull_ExpectArgumentNullException()
            => Assert.ThrowsAsync<ArgumentNullException>(() => CreateSut().ValidateAsync(null));

        [Fact]
        public async Task ValidateAsync_WhenParseRequestThrowsTokenExchangeException_ExpectFailureResult()
        {
            const string expectedError = "invalid_grant"; // TokenRequestErrors.InvalidGrant
            const string expectedErrorMessage = "message from exception";

            var context = new ExtensionGrantValidationContext
            {
                Request = new ValidatedTokenRequest
                {
                    ClientId = "123",
                    Raw = new NameValueCollection {{TokenExchangeConstants.RequestParameters.GrantType, TokenExchangeConstants.GrantType}}
                }
            };

            mockParser.Setup(x => x.Parse(context.Request.ClientId, context.Request.Raw)).Throws(new InvalidRequestException(expectedErrorMessage));
            var sut = CreateSut();

            await sut.ValidateAsync(context);

            context.Result.IsError.Should().BeTrue();
            context.Result.Error.Should().Be(expectedError);
            context.Result.ErrorDescription.Should().Be(expectedErrorMessage);
        }

        [Fact]
        public async Task ValidateAsync_WhenTokenValidatorReturnsInvalid_ExpectFailureResult()
        {
            const string expectedError = "invalid_grant"; // TokenRequestErrors.InvalidGrant
            const string expectedErrorMessage = "Invalid subject token";

            var context = new ExtensionGrantValidationContext
            {
                Request = new ValidatedTokenRequest
                {
                    ClientId = "123",
                    Raw = new NameValueCollection {{TokenExchangeConstants.RequestParameters.GrantType, TokenExchangeConstants.GrantType}}
                }
            };
            
            var mockRequest = new Mock<ITokenExchangeRequest>();
            mockParser.Setup(x => x.Parse(context.Request.ClientId, context.Request.Raw)).Returns(mockRequest.Object);
            mockRequestValidator.Setup(x => x.Validate(It.IsAny<ITokenExchangeRequest>()))
                .ReturnsAsync(TokenExchangeValidationResult.Failure());
            
            var sut = CreateSut();

            await sut.ValidateAsync(context);

            context.Result.IsError.Should().BeTrue();
            context.Result.Error.Should().Be(expectedError);
            context.Result.ErrorDescription.Should().Be(expectedErrorMessage);
        }

        [Fact]
        public async Task ValidateAsync_WhenTokenIsValidAndIssuedToUser_ExpectSuccessResultWithTokenExchangeDefaults()
        {
            var context = new ExtensionGrantValidationContext
            {
                Request = new ValidatedTokenRequest
                {
                    ClientId = "123",
                    Raw = new NameValueCollection {{TokenExchangeConstants.RequestParameters.GrantType, TokenExchangeConstants.GrantType}}
                }
            };

            var subjectClaim = new Claim("sub", "123");
            var nameClaim = new Claim("name", "alice");
            
            var mockRequest = new Mock<ITokenExchangeRequest>();
            mockParser.Setup(x => x.Parse(context.Request.ClientId, context.Request.Raw)).Returns(mockRequest.Object);
            mockRequestValidator.Setup(x => x.Validate(It.IsAny<ITokenExchangeRequest>()))
                .ReturnsAsync(TokenExchangeValidationResult.Success(new[] {subjectClaim, nameClaim}));

            var sut = CreateSut();

            await sut.ValidateAsync(context);

            context.Result.IsError.Should().BeFalse();
            context.Result.Error.Should().BeNull();
            context.Result.ErrorDescription.Should().BeNull();

            context.Result.Subject.Identities.Should().HaveCount(1);
            context.Result.Subject.Identities.Single().AuthenticationType.Should().Be(TokenExchangeConstants.GrantType);
            context.Result.CustomResponse.Should().Contain(new KeyValuePair<string, object>(
                TokenExchangeConstants.ResponseParameters.IssuedTokenType,
                TokenExchangeConstants.TokenTypes.AccessToken));
            
            context.Result.Subject.Claims.Should().ContainSingle(x => x.Type == subjectClaim.Type && x.Value == subjectClaim.Value);
            context.Result.Subject.Claims.Should().ContainSingle(x => x.Type == nameClaim.Type && x.Value == nameClaim.Value);
        }

        [Fact]
        public async Task ValidateAsync_WhenTokenIsValidAndIssuedToClientApp_ExpectSuccessResultWithSubClaimSetToClientId()
        {
            var context = new ExtensionGrantValidationContext
            {
                Request = new ValidatedTokenRequest
                {
                    ClientId = "123",
                    Raw = new NameValueCollection {{"grant_type", TokenExchangeConstants.GrantType}}
                }
            };

            var mockRequest = new Mock<ITokenExchangeRequest>();
            mockParser.Setup(x => x.Parse(context.Request.ClientId, context.Request.Raw)).Returns(mockRequest.Object);
            mockRequestValidator.Setup(x => x.Validate(It.IsAny<ITokenExchangeRequest>()))
                .ReturnsAsync(TokenExchangeValidationResult.Success(new List<Claim>()));

            var sut = CreateSut();

            await sut.ValidateAsync(context);

            context.Result.Subject.FindFirstValue("sub").Should().Be(context.Request.ClientId);
        } 
    }
}