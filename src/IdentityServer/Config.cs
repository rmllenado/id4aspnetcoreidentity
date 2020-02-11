﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
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
                new IdentityResources.Profile()
            };

        public static IEnumerable<ApiResource> Apis =>
            new ApiResource[] 
            { 
                new ApiResource("web_api", "My Web API")
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
                    RedirectUris =  { "http://localhost:5002/signin-oidc" },
                    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },
                    AllowedScopes = new List<string>()
                    {
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OpenId,
                        "web_api"
                    },
                    AllowOfflineAccess = true,
                }
                ,new Client
                {
                    ClientName = "Android Client",
                    ClientId = "native.hybrid",
                    ClientSecrets = { new Secret("secret".ToSha256()) },
                    AllowedGrantTypes = GrantTypes.Code,
                    RequireConsent = false,
                    //RequireClientSecret = true,
                    RequirePkce = true,
                    RefreshTokenUsage = TokenUsage.ReUse,
                    //RedirectUris =  { "http://localhost:5002/signin-oidc" },
                    //PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },
                    AllowedScopes = new List<string>()
                    {
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Email,
                        "web_api"
                    },
                    AllowOfflineAccess = true,
                }
                ,// machine to machine client
                new Client
                {
                    ClientName = "Console Client",
                    ClientId = "client",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    // scopes that client has access to
                    AllowedScopes = { "web_api" }
                },
            };
        
    }
}