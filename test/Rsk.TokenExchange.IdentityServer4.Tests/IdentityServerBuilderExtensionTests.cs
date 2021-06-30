using System.Collections.Generic;
using FluentAssertions;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Rsk.TokenExchange.IdentityServer4.Tests
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