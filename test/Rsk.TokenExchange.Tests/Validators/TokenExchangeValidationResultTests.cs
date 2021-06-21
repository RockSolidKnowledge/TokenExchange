using System.Collections.Generic;
using System.Security.Claims;
using FluentAssertions;
using Rsk.TokenExchange.Validators;
using Xunit;

namespace Rsk.TokenExchange.Tests.Validators
{
    public class TokenExchangeRequestValidationResultTests
    {
        [Fact]
        public void Success_WhenNoClaims_ExpectCorrectProperties()
        {
            var result = TokenExchangeValidationResult.Success();

            result.IsValid.Should().BeTrue();
            result.Claims.Should().BeEmpty();
            result.ErrorDescription.Should().BeNull();
        }

        [Fact]
        public void Success_WhenCustomClaimsIncluded_ExpectCorrectProperties()
        {
            const string expectedSubject = "123";
            var claims = new List<Claim> {new Claim("name", "alice")};

            var result = TokenExchangeValidationResult.Success(claims);

            result.IsValid.Should().BeTrue();
            result.Claims.Should().BeEquivalentTo(claims);
            result.ErrorDescription.Should().BeNull();
        }

        [Fact]
        public void Failure_WhenDescriptionNotIncluded_ExpectCorrectProperties()
        {
            var result = TokenExchangeValidationResult.Failure();

            result.IsValid.Should().BeFalse();
            result.Claims.Should().BeNull();
            result.ErrorDescription.Should().BeNull();
        }

        [Fact]
        public void Failure_WhenDescriptionIncluded_ExpectCorrectProperties()
        {
            const string expectedErrorDescription = "something went wrong";
            
            var result = TokenExchangeValidationResult.Failure(expectedErrorDescription);

            result.IsValid.Should().BeFalse();
            result.Claims.Should().BeNull();
            result.ErrorDescription.Should().Be(expectedErrorDescription);
        }
    }
}