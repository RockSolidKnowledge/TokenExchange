using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Open.IdentityServer.Models;
using Open.IdentityServer.Test;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Open.IdentityServer;
using Rsk.TokenExchange.Open.IdentityServer;

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
        public static readonly Client Client = new()
        {
            ClientId = "client",
            RequireClientSecret = false,
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            AllowedScopes = ["api1", "api2"],
            
            // pretending this is a token from a user interactive flow
            Claims = [new ClientClaim("sub", "1")],
            ClientClaimsPrefix = null
        };
        
        public static readonly Client ApiClient = new()
        {
            ClientId = "api1",
            RequireClientSecret = false,
            AllowedGrantTypes = [TokenExchangeConstants.GrantType],
            AllowedScopes = ["api2"]
        };
    
        public static readonly ApiResource Api1 = new("api1") {Scopes = ["api1"] };
        public static readonly ApiResource Api2 = new("api2") {Scopes = ["api2"] };

        public static readonly TestUser Alice = new() {SubjectId = "1", Username = "alice"};
        public static readonly TestUser Bob = new() {SubjectId = "2", Username = "bob"};
        
        private static readonly ApiScope ApiScope1 = new("api1");
        private static readonly ApiScope ApiScope2 = new("api2");
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentityServer()
                .AddInMemoryClients([Client, ApiClient])
                .AddInMemoryApiResources([Api1, Api2])
                .AddInMemoryApiScopes([ApiScope1, ApiScope2])
                .AddInMemoryIdentityResources(new List<IdentityResource> {new IdentityResources.OpenId(), new IdentityResources.Profile()})
                .AddTestUsers([Alice, Bob])
                .AddSigningCredential(
                    new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP256)),
                    IdentityServerConstants.ECDsaSigningAlgorithm.ES256)
                .AddProfileService<TestProfileService>()
                .AddTokenExchange();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseIdentityServer();
        }
    }

    public class TestProfileService : TestUserProfileService
    {
        public TestProfileService(TestUserStore users, ILogger<TestUserProfileService> logger) : base(users, logger) { }

        public override Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            if (context.Subject.HasClaim(x => x.Type == "act")) context.IssuedClaims.Add(context.Subject.FindFirst("act"));
            return base.GetProfileDataAsync(context);
        }
    }
}