using System;
using System.Collections.Generic;
using System.Security.Claims;
using FluentAssertions;
using Rsk.TokenExchange.Validators;
using Xunit;

namespace Rsk.TokenExchange.Tests.Validators
{
    public class SubjectTokenValidationResultTests
    {
        [Fact]
        public void Success_WhenClaimsCollectionIsNull_ExpectArgumentNullException() 
            => Assert.Throws<ArgumentNullException>(() => SubjectTokenValidationResult.Success(null));

        [Fact]
        public void Success_ExpectIsValidAndClaimsSet()
        {
            var claims = new List<Claim> {new Claim("sub", "123"), new Claim("name", "alice")};

            var result = SubjectTokenValidationResult.Success(claims);

            result.IsValid.Should().BeTrue();
            result.Claims.Should().BeSameAs(claims);
        }

        [Fact]
        public void Failure_ExpectIsValidSetToFalseAndNullClaims()
        {
            var result = SubjectTokenValidationResult.Failure();

            result.IsValid.Should().BeFalse();
            result.Claims.Should().BeNull();
        }
    }
}