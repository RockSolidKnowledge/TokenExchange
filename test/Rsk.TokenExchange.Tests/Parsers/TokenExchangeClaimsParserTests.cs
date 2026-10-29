using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using Rsk.TokenExchange.Exceptions;
using Xunit;

namespace Rsk.TokenExchange.Tests.Parsers
{
    public class TokenExchangeClaimsParserTests
    {
        private readonly Mock<ITokenExchangeRequest> mockRequest = new();
        
        private TokenExchangeClaimsParser CreateSut() => new();

        [Fact]
        public async Task ParseSubject_WhenSubjectClaimsAreNull_ExpectArgumentNullException()
        {
            var sut = CreateSut();
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.ParseSubject(null, mockRequest.Object));
        }
        
        [Fact]
        public async Task ParseSubject_WhenSubjectClaimsAreEmpty_ExpectNull()
        {
            var sut = CreateSut();
            var subject = await sut.ParseSubject(new List<Claim>(), mockRequest.Object);
            subject.Should().BeNull();
        }
        
        [Fact]
        public async Task ParseSubject_WhenSubjectClaimsContainNoSubClaim_ExpectNull()
        {
            var sut = CreateSut();
            var subject = await sut.ParseSubject(new[] {new Claim("name", "alice")}, mockRequest.Object);
            subject.Should().BeNull();
        }
        
        [Fact]
        public async Task ParseSubject_WhenSubjectClaimsContainsSubClaim_ExpectCorrectSubValue()
        {
            const string expectedSub = "123";
            var claims = new[] {new Claim("sub", expectedSub), new Claim("name", "alice")};
            
            var sut = CreateSut();
            var subject = await sut.ParseSubject(claims, mockRequest.Object);
            
            subject.Should().Be(expectedSub);
        }
        
        [Fact]
        public async Task ParseSubject_WhenSubjectClaimsContainsMultiplSubjectClaims_ExpectTokenExchangeException()
        {
            var claims = new[] {new Claim("sub", "123"), new Claim("sub", "xyz"), new Claim("name", "alice")};
            
            var sut = CreateSut();
            await Assert.ThrowsAsync<SubjectParsingException>(() => sut.ParseSubject(claims, mockRequest.Object));
        }

        [Fact]
        public async Task ParseClaims_WhenSubjectClaimsAreNull_ExpectArgumentNullException()
        {
            var sut = CreateSut();
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.ParseClaims(null, mockRequest.Object));
        }

        [Fact]
        public async Task ParseClaims_WhenTokenRequestIsNull_ExpectArgumentNullException()
        {
            var sut = CreateSut();
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.ParseClaims(new List<Claim>(), null));
        }

        [Fact]
        public async Task ParseClaims_WhenNoRulesMet_ExpectSubjectClaimsReturned()
        {
            var claims = new List<Claim> {new Claim("sub", "123"), new Claim("name", "alice")};
            
            var sut = CreateSut();
            var parsedClaims = await sut.ParseClaims(claims, mockRequest.Object);

            parsedClaims.Should().BeEquivalentTo(claims);
        }

        [Fact]
        public async Task ParseClaims_WhenClientIdIsTheSame_ExpectSubjectClaimsReturned()
        {
            const string clientId = "app1";
            var claims = new List<Claim> {new Claim("client_id", clientId), new Claim("name", "alice")};
            mockRequest.Setup(x => x.ClientId).Returns(clientId);
            
            var sut = CreateSut();
            var parsedClaims = await sut.ParseClaims(claims, mockRequest.Object);

            parsedClaims.Should().BeEquivalentTo(claims);
        }

        [Fact]
        public async Task ParseClaims_WhenClientIdIsDifferent_ExpectSubjectClaimsWithActorClaim()
        {
            const string clientId = "api1"; // client of the original token
            const string actorClientId = "app2"; // client making the token exchange request
            var claims = new List<Claim> {new Claim("client_id", clientId), new Claim("name", "alice")};
            mockRequest.Setup(x => x.ClientId).Returns(actorClientId);
            
            var sut = CreateSut();
            var parsedClaims = await sut.ParseClaims(claims, mockRequest.Object);
            
            var actor = parsedClaims.FirstOrDefault(x => x.Type == "act");
            actor.Should().NotBeNull();
            actor?.ValueType.Should().Be(JsonClaimValueTypes.Json);
            
            var parsedActor = JsonSerializer.Deserialize<Actor>(actor?.Value);
            parsedActor.ClientId.Should().Be(actorClientId);
            parsedActor.InnerActor.Should().BeNull();
        }

        [Fact]
        public async Task ParseClaims_WhenClientIdIsDifferentAndActorClaimAlreadyPresent_ExpectSubjectClaimsWithActorClaim()
        {
            const string clientId = "api1"; // client of the original token
            const string actorClientId = "api2"; // client making the token exchange request

            var innerActor = new Actor
            {
                ClientId = "client-1"
            };

            var claims = new List<Claim>
            {
                new Claim("client_id", clientId),
                new Claim("name", "alice"),
                new Claim("act", JsonSerializer.Serialize(innerActor, new JsonSerializerOptions {DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull}), JsonClaimValueTypes.Json)
            };
            mockRequest.Setup(x => x.ClientId).Returns(actorClientId);
            
            var sut = CreateSut();
            var parsedClaims = await sut.ParseClaims(claims, mockRequest.Object);
            
            var actor = parsedClaims.FirstOrDefault(x => x.Type == "act");
            actor.Should().NotBeNull();
            actor?.ValueType.Should().Be(JsonClaimValueTypes.Json);

            if (actor?.Value != null)
            {
                var parsedActor = JsonSerializer.Deserialize<Actor>(actor.Value);
                parsedActor.ClientId.Should().Be(actorClientId);
                parsedActor.InnerActor.Should().BeEquivalentTo(innerActor);
            }
        }
        
        [Fact]
        public async Task ParseClaims_WhenActorContainsNestedActorWhichContainsNestedActor_ShouldCorrectlyNestActorWithinOutputActorClaim()
        {
            const string clientId = "api1"; // client of the original token
            const string actorClientId = "api2"; // client making the token exchange request

            var innerActor = new Actor
            {
                ClientId = "client-1",
                InnerActor = new Actor
                {
                    ClientId = "client-2",
                    InnerActor = new Actor
                    {
                        ClientId = "client-3",
                        InnerActor = new Actor
                        {
                            ClientId = "client-4",
                            InnerActor = new Actor
                            {
                                ClientId = "client-5"
                            }
                        }
                    }
                }
            };

            var claims = new List<Claim>
            {
                new("client_id", clientId),
                new("name", "alice"),
                new("act", JsonSerializer.Serialize(innerActor, new JsonSerializerOptions {DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull}), JsonClaimValueTypes.Json)
            };
            mockRequest.Setup(x => x.ClientId).Returns(actorClientId);
            
            var sut = CreateSut();
            var parsedClaims = await sut.ParseClaims(claims, mockRequest.Object);
            
            var actor = parsedClaims.FirstOrDefault(x => x.Type == "act");
            actor.Should().NotBeNull();
            actor?.ValueType.Should().Be(JsonClaimValueTypes.Json);

            if (actor?.Value != null)
            {
                var parsedActor = JsonSerializer.Deserialize<Actor>(actor.Value);
                parsedActor.ClientId.Should().Be(actorClientId);
                parsedActor.InnerActor.Should().BeEquivalentTo(innerActor);
            }
        }
    }
}