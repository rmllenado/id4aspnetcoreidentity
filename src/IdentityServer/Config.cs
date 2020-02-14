// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            { 
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
            };

        public static IEnumerable<ApiResource> Apis =>
            new ApiResource[]
            {
                new ApiResource("api", "Demo API")
                {
                    ApiSecrets = { new Secret("secret".Sha256()) }
                }
            };
        
        public static IEnumerable<Client> Clients =>
            new Client[] 
            {
                new Client
                {
                    ClientName = "MVC Client",
                    ClientId = "mvc",
                    ClientSecrets = { new Secret("secret".ToSha256()) },
                    AllowedGrantTypes = GrantTypes.Code,
                    RequireConsent = false,
                    RequireClientSecret = true,
                    RequirePkce = true,
                    RedirectUris =  { "https://localhost:5002/signin-oidc" },
                    PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },
                    AllowedScopes = new List<string>()
                    {
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OpenId,
                        "api"
                    },
                    AllowOfflineAccess = true,
                }
                ,new Client
                {
                    ClientName = "Native Client (Hybrid with PKCE)",
                    ClientId = "native.hybrid",

                    RedirectUris =  { "https://notused" },
                    PostLogoutRedirectUris = { "https://notused" },

                    RequireClientSecret = false,
                    RequireConsent = false,
                    
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    RequirePkce = true,
                    AllowedScopes = new List<string>()
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "api"
                    },

                    AllowOfflineAccess = true,

                    RefreshTokenUsage = TokenUsage.ReUse,

                }
                ,// machine to machine client
                new Client
                {
                    ClientName = "Console Client",
                    ClientId = "client",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    // scopes that client has access to
                    AllowedScopes = { "api" }
                },
            };
        
    }
}