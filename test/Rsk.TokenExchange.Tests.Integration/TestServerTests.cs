using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
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

            var subjectTokenResponse = await client.PostAsync("/connect/token", new FormUrlEncodedContent(new []
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", TestStartup.Client.ClientId),
                new KeyValuePair<string, string>("scope", TestStartup.Api1.Name) 
            }));

            subjectTokenResponse.IsSuccessStatusCode.Should().BeTrue();
            var subjectTokenResponseString = await subjectTokenResponse.Content.ReadAsStringAsync();
            var subjectToken = JObject.Parse(subjectTokenResponseString)["access_token"];

            var tokenExchangeResponse = await client.PostAsync("/connect/token", new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", TokenExchangeConstants.GrantType),
                new KeyValuePair<string, string>("client_id", "api1"),
                new KeyValuePair<string, string>("scope", "api2"),

                new KeyValuePair<string, string>(
                    TokenExchangeConstants.RequestParameters.SubjectToken, 
                    subjectToken.Value<string>()),
                new KeyValuePair<string, string>(
                    TokenExchangeConstants.RequestParameters.SubjectTokenType,
                    TokenExchangeConstants.TokenTypes.AccessToken),
            }));

            tokenExchangeResponse.IsSuccessStatusCode.Should().BeTrue();
            var tokenExchangeResponseString = await tokenExchangeResponse.Content.ReadAsStringAsync();
            JObject.Parse(tokenExchangeResponseString)["access_token"].Should().NotBeNull();
        }
        
        [Fact]
        public async Task TokenExchange_WhenUnauthorizedApiSwapsToken_ExpectSuccessfulTokenExchange()
        {
            var client = factory.CreateClient();

            var subjectTokenResponse = await client.PostAsync("/connect/token", new FormUrlEncodedContent(new []
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", TestStartup.Client.ClientId),
                new KeyValuePair<string, string>("scope", TestStartup.Api2.Name) 
            }));

            subjectTokenResponse.IsSuccessStatusCode.Should().BeTrue();
            var subjectTokenResponseString = await subjectTokenResponse.Content.ReadAsStringAsync();
            var subjectToken = JObject.Parse(subjectTokenResponseString)["access_token"];

            var tokenExchangeResponse = await client.PostAsync("/connect/token", new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", TokenExchangeConstants.GrantType),
                new KeyValuePair<string, string>("client_id", "api1"),
                new KeyValuePair<string, string>("scope", "api2"),

                new KeyValuePair<string, string>(
                    TokenExchangeConstants.RequestParameters.SubjectToken, 
                    subjectToken.Value<string>()),
                new KeyValuePair<string, string>(
                    TokenExchangeConstants.RequestParameters.SubjectTokenType,
                    TokenExchangeConstants.TokenTypes.AccessToken),
            }));

            tokenExchangeResponse.IsSuccessStatusCode.Should().BeFalse();
        }
    }
}