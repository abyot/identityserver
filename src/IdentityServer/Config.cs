// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

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

        public static IEnumerable<ApiResource> GetApis(IConfiguration configuration)
        {
            return new List<ApiResource> 
            {
                new ApiResource("bhgapp", $"{configuration["BhgApp:ClientName"]}"),
                new ApiResource("bhgadmin", $"{configuration["BhgAdmin:ClientName"]}")
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

        public static IEnumerable<Client> GetClients(IConfiguration configuration, X509Certificate2 certificate)
        {
            return new List<Client>
            {
                // Barnehage Application Client - IDporten - kindergartenapplication(-test)
                new Client
                {
                    ClientId =  $"{configuration["BhgApp:ClientId"]}",
                    ClientName = $"{configuration["BhgApp:ClientName"]}",
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

                // Barnehage Application Client - IDporten - barnehagesøknad
                new Client
                {
                    ClientId =  $"{configuration["BhgApp2:ClientId"]}",
                    ClientName = $"{configuration["BhgApp2:ClientName"]}",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    AlwaysSendClientClaims = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowOfflineAccess = true,
                    AllowAccessTokensViaBrowser = true,

                    EnableLocalLogin = false,
                    IdentityProviderRestrictions = new List<string> {
                        "idporten2"
                    },

                    RedirectUris =
                    {
                        $"{configuration["BhgApp2:RedirectUri"]}",
                        $"{configuration["BhgApp2:RedirectUri"]}/index.html",
                        $"{configuration["BhgApp2:RedirectUri"]}/callback.html",
                        $"{configuration["BhgApp2:RedirectUri"]}/silent-renew.html"
                    },
                    PostLogoutRedirectUris =
                    {
                        $"{configuration["BhgApp2:PostLogOutRedirectUri"]}",
                        $"{configuration["BhgApp2:PostLogOutRedirectUri"]}/index.html"

                    },
                    AllowedCorsOrigins =     { $"{configuration["BhgApp2:ClientAddress"]}" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "bhgapp"
                    }
                },

                // Barnehage Application Client - IDporten - Certificate
                new Client
                {
                    ClientId =  $"{configuration["BhgApp3:ClientId"]}",
                    ClientName = $"{configuration["BhgApp3:ClientName"]}",
                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,

                    ClientSecrets =
                    {
                        new Secret
                        {
                            Type = IdentityServerConstants.SecretTypes.X509CertificateBase64,
                            Value = Convert.ToBase64String(certificate.GetRawCertData())
                        }
                    },

                    RequirePkce = true,
                    RequireClientSecret = false,
                    AlwaysSendClientClaims = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowOfflineAccess = true,
                    AllowAccessTokensViaBrowser = true,

                    EnableLocalLogin = false,
                    IdentityProviderRestrictions = new List<string> {
                        "idporten3"
                    },

                    RedirectUris =
                    {
                        $"{configuration["BhgApp3:RedirectUri"]}",
                        $"{configuration["BhgApp3:RedirectUri"]}/index.html",
                        $"{configuration["BhgApp3:RedirectUri"]}/callback.html",
                        $"{configuration["BhgApp3:RedirectUri"]}/silent-renew.html"
                    },
                    PostLogoutRedirectUris =
                    {
                        $"{configuration["BhgApp3:PostLogOutRedirectUri"]}",
                        $"{configuration["BhgApp3:PostLogOutRedirectUri"]}/index.html"

                    },
                    AllowedCorsOrigins =     { $"{configuration["BhgApp3:ClientAddress"]}" },

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
                    ClientId =  $"{configuration["BhgAdmin:ClientId"]}",
                    ClientName = $"{configuration["BhgAdmin:ClientName"]}",
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