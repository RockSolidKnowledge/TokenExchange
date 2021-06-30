using System.Collections.Generic;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Rsk.TokenExchange.DuendeIdentityServer.Tests
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