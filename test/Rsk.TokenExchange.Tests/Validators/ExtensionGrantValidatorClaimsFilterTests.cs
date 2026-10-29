using System;
using System.Security.Claims;
using FluentAssertions;
using Xunit;

namespace Rsk.TokenExchange.Tests.Validators
{
    public class ExtensionGrantValidatorClaimsFilterTests
    {
        private ExtensionGrantValidatorClaimsFilter CreateSut() => new();

        [Fact]
        public void ShouldIncludeClaim_WhenClaimIsNull_ExpectArgumentNullException()
        {
            var sut = CreateSut();

            Action act = () => sut.ShouldIncludeClaim(null);

            act.Should().Throw<ArgumentNullException>();
        }
        
        [Theory]
        [InlineData("sub")]
        [InlineData("idp")]
        [InlineData("auth_time")]
        public void ShouldIncludeClaim_WhenFrameworkOwnedIdentityClaim_ExpectFalse(string claimType)
        {
            var sut = CreateSut();

            var result = sut.ShouldIncludeClaim(new Claim(claimType, "value"));

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldIncludeClaim_WhenRegularClaim_ExpectTrue()
        {
            var sut = CreateSut();

            var result = sut.ShouldIncludeClaim(new Claim("name", "alice"));

            result.Should().BeTrue();
        }
    }
}

