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
using Rsk.TokenExchange.Validators;
using Xunit;

namespace Rsk.TokenExchange.IdentityServer4.Tests
{
    public class TokenExchangeExtensionGrantValidatorTests
    {
        private Mock<ITokenExchangeRequestParser> mockParser = new Mock<ITokenExchangeRequestParser>();
        private Mock<ITokenExchangeRequestValidator> mockRequestValidator = new Mock<ITokenExchangeRequestValidator>();
        private Mock<ITokenExchangeClaimsParser> mockClaimsParser = new Mock<ITokenExchangeClaimsParser>();
        private ILogger<TokenExchangeExtensionGrantValidator> logger = new NullLogger<TokenExchangeExtensionGrantValidator>();

        private ExtensionGrantValidationContext context = new ExtensionGrantValidationContext
        {
            Request = new ValidatedTokenRequest
            {
                ClientId = "123",
                Raw = new NameValueCollection {{TokenExchangeConstants.RequestParameters.GrantType, TokenExchangeConstants.GrantType}}
            }
        };

        private TokenExchangeExtensionGrantValidator CreateSut()
            => new TokenExchangeExtensionGrantValidator(mockParser?.Object, mockRequestValidator?.Object, mockClaimsParser?.Object, logger);
        
        private Mock<TokenExchangeExtensionGrantValidator> CreateMockedSut()
        {
            var sut = new Mock<TokenExchangeExtensionGrantValidator>(mockParser?.Object, mockRequestValidator?.Object, mockClaimsParser?.Object, logger)
            {
                CallBase = true
            };

            sut.Setup(x => x.UpdateRequest(It.IsAny<ValidatedTokenRequest>(), It.IsAny<IEnumerable<Claim>>()));

            return sut;
        }

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
        public void ctor_WhenClaimsParserIsNull_ExpectArgumentNullException()
        {
            mockClaimsParser = null;
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
            => Assert.ThrowsAsync<ArgumentNullException>(() => CreateMockedSut().Object.ValidateAsync(null));

        [Fact]
        public async Task ValidateAsync_WhenParseRequestThrowsTokenExchangeException_ExpectFailureResult()
        {
            const string expectedError = "invalid_grant"; // TokenRequestErrors.InvalidGrant
            const string expectedErrorMessage = "message from exception";

            mockParser.Setup(x => x.Parse(context.Request.ClientId, context.Request.Raw))
                .Throws(new InvalidRequestException(expectedErrorMessage));
            var sut = CreateMockedSut();

            await sut.Object.ValidateAsync(context);

            context.Result.IsError.Should().BeTrue();
            context.Result.Error.Should().Be(expectedError);
            context.Result.ErrorDescription.Should().Be(expectedErrorMessage);
        }

        [Fact]
        public async Task ValidateAsync_WhenTokenValidatorReturnsInvalid_ExpectFailureResult()
        {
            const string expectedError = "invalid_grant"; // TokenRequestErrors.InvalidGrant
            const string expectedErrorMessage = "Invalid subject token";

            var mockRequest = new Mock<ITokenExchangeRequest>();
            mockParser.Setup(x => x.Parse(context.Request.ClientId, context.Request.Raw))
                .Returns(mockRequest.Object);
            mockRequestValidator.Setup(x => x.Validate(It.IsAny<ITokenExchangeRequest>()))
                .ReturnsAsync(TokenExchangeValidationResult.Failure());
            
            var sut = CreateMockedSut();

            await sut.Object.ValidateAsync(context);

            context.Result.IsError.Should().BeTrue();
            context.Result.Error.Should().Be(expectedError);
            context.Result.ErrorDescription.Should().Be(expectedErrorMessage);
        }

        [Fact]
        public async Task ValidateAsync_WhenParseSubjectThrowsTokenExchangeException_ExpectFailureResult()
        {
            const string expectedError = "invalid_grant"; // TokenRequestErrors.InvalidGrant

            var claims = new List<Claim> {new Claim("sub", "123")};
            var exception = new SubjectParsingException("error!");
            
            var mockRequest = new Mock<ITokenExchangeRequest>();
            mockParser.Setup(x => x.Parse(context.Request.ClientId, context.Request.Raw))
                .Returns(mockRequest.Object);
            mockRequestValidator.Setup(x => x.Validate(It.IsAny<ITokenExchangeRequest>()))
                .ReturnsAsync(TokenExchangeValidationResult.Success(claims));
            mockClaimsParser.Setup(x => x.ParseSubject(claims, mockRequest.Object))
                .Throws(exception);
            
            var sut = CreateMockedSut();

            await sut.Object.ValidateAsync(context);

            context.Result.IsError.Should().BeTrue();
            context.Result.Error.Should().Be(expectedError);
            context.Result.ErrorDescription.Should().Contain(exception.GetType().ToString());
            context.Result.ErrorDescription.Should().Contain(exception.Message);
        }

        [Fact]
        public async Task ValidateAsync_WhenParseSubjectReturnsNull_ExpectFailureResult()
        {
            const string expectedError = "invalid_grant"; // TokenRequestErrors.InvalidGrant
            const string expectedErrorMessage = "Unable to parse subject claim";

            var claims = new List<Claim> {new Claim("sub", "123")};
            
            var mockRequest = new Mock<ITokenExchangeRequest>();
            mockParser.Setup(x => x.Parse(context.Request.ClientId, context.Request.Raw))
                .Returns(mockRequest.Object);
            mockRequestValidator.Setup(x => x.Validate(It.IsAny<ITokenExchangeRequest>()))
                .ReturnsAsync(TokenExchangeValidationResult.Success(claims));
            mockClaimsParser.Setup(x => x.ParseSubject(claims, mockRequest.Object))
                .ReturnsAsync((string) null);
            
            var sut = CreateMockedSut();

            await sut.Object.ValidateAsync(context);

            context.Result.IsError.Should().BeTrue();
            context.Result.Error.Should().Be(expectedError);
            context.Result.ErrorDescription.Should().Contain(expectedErrorMessage);
        }

        [Fact]
        public async Task ValidateAsync_WhenTokenIsValidAndIssuedToUser_ExpectSuccessResultWithTokenExchangeDefaults()
        {
            var subjectClaim = new Claim("sub", "123");
            var nameClaim = new Claim("name", "alice");
            var claims = new List<Claim>{subjectClaim, nameClaim};

            var expectedAdditionalClaim = new Claim("act", "test");
            
            var mockRequest = new Mock<ITokenExchangeRequest>();
            mockParser.Setup(x => x.Parse(context.Request.ClientId, context.Request.Raw))
                .Returns(mockRequest.Object);
            mockRequestValidator.Setup(x => x.Validate(It.IsAny<ITokenExchangeRequest>()))
                .ReturnsAsync(TokenExchangeValidationResult.Success(claims));
            mockClaimsParser.Setup(x => x.ParseSubject(claims, mockRequest.Object))
                .ReturnsAsync(subjectClaim.Value);
            mockClaimsParser.Setup(x => x.ParseClaims(claims, mockRequest.Object))
                .ReturnsAsync(new[] {expectedAdditionalClaim});

            var sut = CreateMockedSut();

            await sut.Object.ValidateAsync(context);

            context.Result.IsError.Should().BeFalse();
            context.Result.Error.Should().BeNull();
            context.Result.ErrorDescription.Should().BeNull();

            context.Result.Subject.Identities.Should().HaveCount(1);
            context.Result.Subject.Identities.Single().AuthenticationType.Should().Be(TokenExchangeConstants.GrantType);
            context.Result.CustomResponse.Should().Contain(new KeyValuePair<string, object>(
                TokenExchangeConstants.ResponseParameters.IssuedTokenType,
                TokenExchangeConstants.TokenTypes.AccessToken));
            
            context.Result.Subject.Claims.Should().ContainSingle(x => x.Type == subjectClaim.Type && x.Value == subjectClaim.Value);
            context.Result.Subject.Claims.Should().ContainSingle(x => x.Type == expectedAdditionalClaim.Type && x.Value == expectedAdditionalClaim.Value);
            
            sut.Verify(x => x.UpdateRequest(context.Request, It.IsAny<IEnumerable<Claim>>()), Times.Once);
        }

        [Fact]
        public void UpdateRequest_WhenRequestIsNull_ExpectArgumentNullException()
        {
            var sut = CreateSut();
            Assert.Throws<ArgumentNullException>(() => sut.UpdateRequest(null, new List<Claim> {new Claim("client_id", "123")}));
        }

        [Fact]
        public void UpdateRequest_WhenClaimsAreNull_ExpectArgumentNullException()
        {
            var sut = CreateSut();
            Assert.Throws<ArgumentNullException>(() => sut.UpdateRequest(new ValidatedTokenRequest(), null));
        }

        [Fact]
        public void UpdateRequest_ExpectRequestClientIdUpdate()
        {
            const string expectedClientId = "xyz";
            var request = new ValidatedTokenRequest {ClientId = Guid.NewGuid().ToString()};

            var sut = CreateSut();
            sut.UpdateRequest(request, new[] {new Claim("client_id", expectedClientId)});

            request.ClientId.Should().Be(expectedClientId);
        }
    }
}