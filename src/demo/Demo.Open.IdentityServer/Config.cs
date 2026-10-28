// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer;
using Open.IdentityServer.Models;
using Rsk.TokenExchange;
using System.Collections.Generic;

namespace Open.IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };


        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("api1", "My API"),
                new ApiScope("api2", "My API")
            };

        // Gives api1 access tokens aud=client-2, so client-2 is an intended recipient
        // of the subject token and is allowed to exchange it.
        public static IEnumerable<ApiResource> ApiResources =>
            new List<ApiResource>
            {
                new ApiResource("client-2", "Client 2 (API)") { Scopes = { "api1" } },
                // Gives api2 access tokens aud=client-3, so client-3 can exchange a token issued for api2 (nested act repro).
                new ApiResource("client-3", "Client 3 (API)") { Scopes = { "api2" } }
            };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                new Client
                {
                    ClientId = "client-1",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = new[] { "authorization_code", TokenExchangeConstants.GrantType },
                    RequirePkce = false,
                    RedirectUris = { "https://localhost:5002/signin-oidc" },

                    AllowedScopes = { "openid", "profile", "api1", "api2" }
                },
                new Client
                {
                    ClientId = "client-2",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = { TokenExchangeConstants.GrantType },

                    AllowedScopes = { "api2" }
                },
                new Client
                {
                    ClientId = "client-3",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = { TokenExchangeConstants.GrantType },

                    AllowedScopes = { "api2" }
                }
            };
    }
}