using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.IdentityModel.JsonWebTokens;
using Xunit;

namespace Rsk.TokenExchange.Tests.Integration
{
    public class TestServerTests : IClassFixture<CustomWebApplicationFactory<TestStartup>>
    {
        private readonly CustomWebApplicationFactory<TestStartup> factory;

        public TestServerTests(CustomWebApplicationFactory<TestStartup> factory)
        {
            this.factory = factory;
        }
        
        [Fact]
        public async Task TokenExchange_WhenAuthorizedApiSwapsToken_ExpectSuccessfulTokenExchange()
        {
            var client = factory.CreateClient();

            var subjectTokenResponse = await client.PostAsync("/connect/token", new FormUrlEncodedContent([
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", TestStartup.Client.ClientId),
                new KeyValuePair<string, string>("scope", TestStartup.Api1.Name)
            ]));

            subjectTokenResponse.IsSuccessStatusCode.Should().BeTrue();
            var subjectTokenResponseString = await subjectTokenResponse.Content.ReadAsStringAsync();
            var subjectToken = GetStringProperty(subjectTokenResponseString, "access_token");

            var tokenExchangeResponse = await client.PostAsync("/connect/token", new FormUrlEncodedContent([
                new KeyValuePair<string, string>("grant_type", TokenExchangeConstants.GrantType),
                new KeyValuePair<string, string>("client_id", TestStartup.Api1.Name),
                new KeyValuePair<string, string>("scope", TestStartup.Api2.Name),

                new KeyValuePair<string, string>(
                    TokenExchangeConstants.RequestParameters.SubjectToken, 
                    subjectToken),
                new KeyValuePair<string, string>(
                    TokenExchangeConstants.RequestParameters.SubjectTokenType,
                    TokenExchangeConstants.TokenTypes.AccessToken)
            ]));

            tokenExchangeResponse.IsSuccessStatusCode.Should().BeTrue();
            var tokenExchangeResponseString = await tokenExchangeResponse.Content.ReadAsStringAsync();
            GetStringProperty(tokenExchangeResponseString, "access_token").Should().NotBeNullOrEmpty();
        }
        
        [Fact]
        public async Task TokenExchange_WhenUnauthorizedApiSwapsToken_ExpectFailure()
        {
            var client = factory.CreateClient();

            var subjectTokenResponse = await client.PostAsync("/connect/token", new FormUrlEncodedContent([
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", TestStartup.Client.ClientId),
                new KeyValuePair<string, string>("scope", TestStartup.Api2.Name)
            ]));

            subjectTokenResponse.IsSuccessStatusCode.Should().BeTrue();
            var subjectTokenResponseString = await subjectTokenResponse.Content.ReadAsStringAsync();
            var subjectToken = GetStringProperty(subjectTokenResponseString, "access_token");

            var tokenExchangeResponse = await client.PostAsync("/connect/token", new FormUrlEncodedContent([
                new KeyValuePair<string, string>("grant_type", TokenExchangeConstants.GrantType),
                new KeyValuePair<string, string>("client_id", TestStartup.Api1.Name),
                new KeyValuePair<string, string>("scope", TestStartup.Api2.Name),

                new KeyValuePair<string, string>(
                    TokenExchangeConstants.RequestParameters.SubjectToken, 
                    subjectToken),
                new KeyValuePair<string, string>(
                    TokenExchangeConstants.RequestParameters.SubjectTokenType,
                    TokenExchangeConstants.TokenTypes.AccessToken)
            ]));

            tokenExchangeResponse.IsSuccessStatusCode.Should().BeFalse();
        }

        [Fact]
        public async Task TokenExchange_ExpectCorrectClientIdAndActorClaim()
        {
            var accessToken = await TokenExchange();

            JsonWebToken token = new JsonWebTokenHandler().ReadJsonWebToken(accessToken);
            
            // original client app maintained as client_id
            token.Claims.Should().Contain(x => x.Type == "client_id", TestStartup.Client.ClientId);
            
            // actor claim containing client app acting on their behalf
            token.Claims.Should().Contain(x => x.Type == "act");
            var actorClaim = token.Claims.First(x => x.Type == "act");
            var actorClientId = GetStringProperty(actorClaim.Value, "client_id");
            actorClientId.Should().Be(TestStartup.Api1.Name);
        }

        private async Task<string> TokenExchange()
        {
            var client = factory.CreateClient();

            var subjectTokenResponse = await client.PostAsync("/connect/token", new FormUrlEncodedContent([
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", TestStartup.Client.ClientId),
                new KeyValuePair<string, string>("scope", TestStartup.Api1.Name)
            ]));

            subjectTokenResponse.IsSuccessStatusCode.Should().BeTrue();
            var subjectTokenResponseString = await subjectTokenResponse.Content.ReadAsStringAsync();
            var subjectToken = GetStringProperty(subjectTokenResponseString, "access_token");

            var tokenExchangeResponse = await client.PostAsync("/connect/token", new FormUrlEncodedContent([
                new KeyValuePair<string, string>("grant_type", TokenExchangeConstants.GrantType),
                new KeyValuePair<string, string>("client_id", TestStartup.Api1.Name),
                new KeyValuePair<string, string>("scope", TestStartup.Api2.Name),

                new KeyValuePair<string, string>(
                    TokenExchangeConstants.RequestParameters.SubjectToken,
                    subjectToken),
                new KeyValuePair<string, string>(
                    TokenExchangeConstants.RequestParameters.SubjectTokenType,
                    TokenExchangeConstants.TokenTypes.AccessToken)
            ]));

            tokenExchangeResponse.IsSuccessStatusCode.Should().BeTrue();
            var tokenExchangeResponseString = await tokenExchangeResponse.Content.ReadAsStringAsync();

            return GetStringProperty(tokenExchangeResponseString, "access_token");
        }
        
        [Fact]
        public async Task TokenExchange_ClientIdClaim_ShouldBeRequestingClient()
        {
            var accessToken = await TokenExchange();

            JsonWebToken token = new JsonWebTokenHandler().ReadJsonWebToken(accessToken);
            var payload = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Decode(token.EncodedPayload);

            token.GetClaim("client_id").Value.Should().Be(
                TestStartup.ApiClient.ClientId,
                "RFC 8693 §4.3 says client_id is the client that requested the token. Issued token payload: {0}",
                payload);
        }

        private static string GetStringProperty(string json, string propertyName)
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.GetProperty(propertyName).GetString();
        }
    }
}