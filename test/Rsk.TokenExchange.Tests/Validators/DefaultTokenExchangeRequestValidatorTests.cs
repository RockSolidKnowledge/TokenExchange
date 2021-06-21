using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Rsk.TokenExchange.Validators;
using Xunit;

namespace Rsk.TokenExchange.Tests.Validators
{
    public class DefaultTokenExchangeRequestValidatorTests
    {
        private Mock<ISubjectTokenValidator> mockSubjectTokenValidator = new Mock<ISubjectTokenValidator>();

        private readonly Mock<ITokenExchangeRequest> mockRequest;
        private readonly Mock<ISubjectTokenValidationResult> mockSubjectTokenResponse;

        public DefaultTokenExchangeRequestValidatorTests()
        {
            const string testClientId = "api4";
            
            mockRequest = new Mock<ITokenExchangeRequest>();
            mockRequest.Setup(x => x.ClientId).Returns(testClientId);

            mockSubjectTokenResponse = new Mock<ISubjectTokenValidationResult>();
            mockSubjectTokenResponse.Setup(x => x.Claims).Returns(new[] {new Claim("aud", testClientId)});
            
            mockSubjectTokenValidator.Setup(x => x.Validate(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(mockSubjectTokenResponse.Object);
        }
        
        private DefaultTokenExchangeRequestValidator CreateSut() 
            => new DefaultTokenExchangeRequestValidator(mockSubjectTokenValidator?.Object);

        [Fact]
        public void ctor_WhenSubjectTokenValidatorIsNull_ExpectArgumentNullException()
        {
            mockSubjectTokenValidator = null;
            Assert.Throws<ArgumentNullException>(CreateSut);
        }
        
        [Fact]
        public async Task Validate_WhenRequestIsNull_ExpectArgumentNullException()
        {
            var sut = CreateSut();
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.Validate(null));
        }

        [Fact]
        public async Task Validate_ExpectSubjectTokenValidatorCalledWithCorrectParameters()
        {
            const string expectedSubjectToken = "eyj.eyj";
            const string expectedSubjectTokenType = TokenExchangeConstants.TokenTypes.AccessToken;
            
            mockRequest.Setup(x => x.SubjectToken).Returns(expectedSubjectToken);
            mockRequest.Setup(x => x.SubjectTokenType).Returns(expectedSubjectTokenType);

            mockSubjectTokenResponse.Setup(x => x.IsValid).Returns(true);
            
            mockSubjectTokenValidator.Setup(x => x.Validate(expectedSubjectToken, expectedSubjectTokenType))
                .ReturnsAsync(mockSubjectTokenResponse.Object)
                .Verifiable();

            var sut = CreateSut();
            await sut.Validate(mockRequest.Object);
            
            mockSubjectTokenValidator.Verify();
        }

        [Fact]
        public async Task Validate_WhenSubjectTokenValidatorFails_ExpectFailureResult()
        {
            mockSubjectTokenResponse.Setup(x => x.IsValid).Returns(false);

            var sut = CreateSut();
            var result = await sut.Validate(mockRequest.Object);

            result.IsValid.Should().BeFalse();
            result.Claims.Should().BeNull();
            result.ErrorDescription.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Validate_WhenSubjectTokenIsValidWithInvalidAudience_ExpectFailureResult()
        {
            mockRequest.Setup(x => x.ClientId).Returns(Guid.NewGuid().ToString);
            
            mockSubjectTokenResponse.Setup(x => x.IsValid).Returns(true);
            mockSubjectTokenResponse.Setup(x => x.Claims).Returns(new[] {new Claim("aud", Guid.NewGuid().ToString())});

            var sut = CreateSut();
            var result = await sut.Validate(mockRequest.Object);

            result.IsValid.Should().BeFalse();
            result.Claims.Should().BeNull();
            result.ErrorDescription.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Validate_WhenSubjectTokenIsValidWithCorrectAudience_ExpectSuccessResult()
        {
            const string validAudience = "api1";
            
            var expectedClaims = new List<Claim>
            {
                new Claim("sub", "123"),
                new Claim("name", "alice"),
                new Claim("aud", validAudience)
            };
            
            mockRequest.Setup(x => x.ClientId).Returns(validAudience);
            
            mockSubjectTokenResponse.Setup(x => x.IsValid).Returns(true);
            mockSubjectTokenResponse.Setup(x => x.Claims).Returns(expectedClaims);

            var sut = CreateSut();
            var result = await sut.Validate(mockRequest.Object);

            result.IsValid.Should().BeTrue();
            result.Claims.Should().BeSameAs(expectedClaims);
            result.ErrorDescription.Should().BeNull();
        }

        [Fact]
        public async Task Validate_WhenSubjectTokenIsValidWithCorrectAudienceAndMultipleAudiences_ExpectSuccessResult()
        {
            const string validAudience = "api1";
            
            var expectedClaims = new List<Claim>
            {
                new Claim("sub", "123"),
                new Claim("name", "alice"),
                new Claim("aud", validAudience),
                new Claim("aud", "api4")
            };
            
            mockRequest.Setup(x => x.ClientId).Returns(validAudience);
            
            mockSubjectTokenResponse.Setup(x => x.IsValid).Returns(true);
            mockSubjectTokenResponse.Setup(x => x.Claims).Returns(expectedClaims);

            var sut = CreateSut();
            var result = await sut.Validate(mockRequest.Object);

            result.IsValid.Should().BeTrue();
            result.Claims.Should().BeSameAs(expectedClaims);
            result.ErrorDescription.Should().BeNull();
        }
    }
}