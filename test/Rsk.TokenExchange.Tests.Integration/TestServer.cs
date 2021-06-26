using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Rsk.TokenExchange.IdentityServer;
using Rsk.TokenExchange.Validators;

namespace Rsk.TokenExchange.Tests.Integration
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(host =>
                {
                    host.UseStartup<TStartup>();
                    host.UseTestServer();
                });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseContentRoot(Directory.GetCurrentDirectory());
            return base.CreateHost(builder);
        }
    }

    public class TestStartup
    {
        public static readonly Client Client = new Client
        {
            ClientId = "client",
            RequireClientSecret = false,
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            AllowedScopes = new[] {"api1", "api2"},
            
            // pretending this is a token from a user interactive flow
            Claims = new[] {new ClientClaim("sub", "1")},
            ClientClaimsPrefix = null
        };
        
        public static readonly Client ApiClient = new Client
        {
            ClientId = "api1",
            RequireClientSecret = false,
            AllowedGrantTypes = new[] {TokenExchangeConstants.GrantType},
            AllowedScopes = new[] {"api2"}
        };
    
        public static readonly ApiResource Api1 = new ApiResource("api1") {Scopes = new[] {"api1"}};
        public static readonly ApiResource Api2 = new ApiResource("api2") {Scopes = new[] {"api2"}};

        public static readonly TestUser Alice = new TestUser {SubjectId = "1", Username = "alice"};
        public static readonly TestUser Bob = new TestUser {SubjectId = "2", Username = "bob"};
        
        private static readonly ApiScope ApiScope1 = new ApiScope("api1");
        private static readonly ApiScope ApiScope2 = new ApiScope("api2");
        
        public void ConfigureServices(IServiceCollection services)
        {
            var builder = services.AddIdentityServer()
                .AddInMemoryClients(new[] {Client, ApiClient})
                .AddInMemoryApiResources(new[] {Api1, Api2})
                .AddInMemoryApiScopes(new[] {ApiScope1, ApiScope2})
                .AddInMemoryIdentityResources(new List<IdentityResource> {new IdentityResources.OpenId(), new IdentityResources.Profile()})
                .AddTestUsers(new List<TestUser> {Alice, Bob})
                .AddSigningCredential(
                    new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP256)),
                    IdentityServerConstants.ECDsaSigningAlgorithm.ES256);

            builder.AddExtensionGrantValidator<TokenExchangeExtensionGrantValidator>();
            services.AddTransient<ITokenExchangeRequestParser, TokenExchangeRequestParser>();
            services.AddTransient<ITokenExchangeRequestValidator, DefaultTokenExchangeRequestValidator>();
            services.AddTransient<ISubjectTokenValidator, IdentityServerSubjectTokenValidator>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseIdentityServer();
        }
    }
}