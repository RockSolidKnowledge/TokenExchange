using System.Collections.Generic;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Open.IdentityServer.Models;
using Open.IdentityServer.Validation;
using Xunit;

namespace Rsk.TokenExchange.Open.IdentityServer.Tests
{
    public class IdentityServerBuilderExtensionTests
    {
        [Fact]
        public void AddTokenExchange_ExpectExtensionGrantResolvable()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddIdentityServer()
                .AddInMemoryClients(new List<Client>())
                .AddInMemoryApiResources(new List<ApiResource>())
                .AddInMemoryApiScopes(new List<ApiScope>())
                .AddInMemoryIdentityResources(new List<IdentityResource>())
                .AddDeveloperSigningCredential()
                .AddTokenExchange();

            var services = serviceCollection.BuildServiceProvider();
            var grantValidators = services.GetServices<IExtensionGrantValidator>();
            grantValidators.Should().Contain(x => x.GrantType == TokenExchangeConstants.GrantType);
        }
    }
}