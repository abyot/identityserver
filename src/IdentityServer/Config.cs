// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource> 
            {
                new ApiResource("bhgapp", "Barnehage Application"),
                new ApiResource("bhgadmin", "Barnehage Application Administration")
            };
        }

        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "alice",
                    Password = "password",

                    Claims = new []
                    {
                        new Claim("name", "Alice"),
                        new Claim("website", "https://alice.com")
                    }
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "bob",
                    Password = "password",

                    Claims = new []
                    {
                        new Claim("name", "Bob"),
                        new Claim("website", "https://bob.com")
                    }
                }
            };
        }

        public static IEnumerable<Client> GetClients(IConfiguration configuration)
        {
            return new List<Client>
            {                
                // Barnehage Application Client - IDporten
                new Client
                {
                    ClientId = "5716ae6a-4995-4830-89a2-0fd61426aba5",
                    ClientName = "Barnehage Application",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    AlwaysSendClientClaims = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowOfflineAccess = true,
                    AllowAccessTokensViaBrowser = true,
                    
                    EnableLocalLogin = false,
                    IdentityProviderRestrictions = new List<string> {
                        "idporten"
                    },

                    RedirectUris =
                    {
                        $"{configuration["BhgApp:RedirectUri"]}",
                        $"{configuration["BhgApp:RedirectUri"]}/index.html",
                        $"{configuration["BhgApp:RedirectUri"]}/callback.html",
                        $"{configuration["BhgApp:RedirectUri"]}/silent-renew.html"
                    },
                    PostLogoutRedirectUris =
                    {
                        $"{configuration["BhgApp:PostLogOutRedirectUri"]}",
                        $"{configuration["BhgApp:PostLogOutRedirectUri"]}/index.html"

                    },
                    AllowedCorsOrigins =     { $"{configuration["BhgApp:ClientAddress"]}" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "bhgapp"
                    }
                },

                // Barnehage Application Administration Client - AAD
                new Client
                {
                    ClientId = "d470c053-1185-4cea-8d10-91ac6e27391e",
                    ClientName = "Barnehage Application Administration",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    AlwaysSendClientClaims = true,
                    AllowOfflineAccess = true,
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true,

                    EnableLocalLogin = false,
                    IdentityProviderRestrictions = new List<string> {
                        "aad"
                    },

                    RedirectUris =
                    {
                        $"{configuration["BhgAdmin:RedirectUri"]}",
                        $"{configuration["BhgAdmin:RedirectUri"]}/index.html",
                        $"{configuration["BhgAdmin:RedirectUri"]}/callback.html",
                        $"{configuration["BhgAdmin:RedirectUri"]}/silent-renew.html"
                    },
                    PostLogoutRedirectUris =
                    {
                        $"{configuration["BhgAdmin:PostLogOutRedirectUri"]}",
                        $"{configuration["BhgAdmin:PostLogOutRedirectUri"]}/index.html"
                    },
                    AllowedCorsOrigins =     { $"{configuration["BhgAdmin:ClientAddress"]}" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "bhgadmin",
                    }
                }
            };
        }
    }
}