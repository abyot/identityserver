using System;
using IdentityServer4;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer.AppConfig;

namespace IdentityServer
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            X509Certificate2 certificate = GetCertificate();

            X509SecurityKey privateKey = new X509SecurityKey(certificate);

            var jwtToken = GenerateJWT(certificate, Configuration["BhgApp3:ClientId"]);

            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            var builder = services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApis(Configuration))
                .AddInMemoryClients(Config.GetClients(Configuration, certificate))
                .AddTestUsers(Config.GetUsers())
                .AddProfileService<ProfileService>();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddSingleton(_ => Configuration);

            services.AddAuthentication()
                //ID-porten - kindergartenapplication(-test) username:password client
                .AddOpenIdConnect("idporten", "ID Porten", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.Authority = Configuration["BhgApp:Authority"];
                    options.ClientId = Configuration["BhgApp:ClientId"];
                    options.ClientSecret = Configuration["BhgApp:ClientSecret"];
                    options.ResponseType = "code";
                    options.CallbackPath = "/signin-idporten";
                    options.SignedOutCallbackPath = "/signout-callback-idporten";
                    options.SignedOutRedirectUri = Configuration["BhgApp:SignedOutRedirectUri"];
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;
                })
                //IDporten - barnehagesoknad username:password client
                .AddOpenIdConnect("idporten2", "ID Porten", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.Authority = Configuration["BhgApp2:Authority"];
                    options.ClientId = Configuration["BhgApp2:ClientId"];
                    options.ClientSecret = Configuration["BhgApp2:ClientSecret"];
                    options.ResponseType = "code";
                    options.CallbackPath = "/signin-bhs";
                    options.SignedOutCallbackPath = "/signout-callback-bhs";
                    options.SignedOutRedirectUri = Configuration["BhgApp2:SignedOutRedirectUri"];
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;
                })
                //IDporten - barnehagesoknad certificate based client
                .AddOpenIdConnect("idporten3", "ID Porten", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.Authority = Configuration["BhgApp3:Authority"];
                    options.ClientId = Configuration["BhgApp3:ClientId"];
                    options.ResponseType = "code";
                    options.CallbackPath = "/signin-bhs-test";
                    options.SignedOutCallbackPath = "/signout-callback-bhs-test";
                    options.SignedOutRedirectUri = Configuration["BhgApp3:SignedOutRedirectUri"];
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    options.Events.OnAuthorizationCodeReceived = context =>
                    {
                        context.TokenEndpointRequest.ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
                        context.TokenEndpointRequest.ClientAssertion = jwtToken;

                        return Task.CompletedTask;
                    };
                })
                //Azure AD client
                .AddOpenIdConnect("aad", "Azure AD", options =>
                {
                    options.Authority = Configuration["BhgAdmin:Authority"];
                    options.ClientId = Configuration["BhgAdmin:ClientId"];
                    options.ClientSecret = Configuration["BhgAdmin:ClientSecret"];
                    options.CallbackPath = "/signin-aad";
                    options.SignedOutCallbackPath = "/signout-callback-aad";
                    options.ResponseType = "code";
                    options.SignedOutRedirectUri = Configuration["BhgAdmin:SignedOutRedirectUri"];
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SignOutScheme = IdentityServerConstants.SignoutScheme;
                    options.SaveTokens = true;
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();

            AppConfiguration.SetConfig(Configuration);

            app.UseIdentityServer();

            app.UseMvcWithDefaultRoute();
        }

        private X509Certificate2 GetCertificate()
        {
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(async (authority, resource, scope) =>
            {
                var authContext = new AuthenticationContext(authority);

                ClientCredential clientCreds = new ClientCredential(Configuration["DimeIdentityClientId"], Configuration["DimeIdentityClientSecret"]);

                AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCreds);

                if (result == null)
                {
                    throw new InvalidOperationException("Failed to obtain the JWT token");
                }

                return result.AccessToken;
            }));

            var certificateSecret = keyVaultClient.GetSecretAsync($"https://{Configuration["DimeKeyVaultAddress"]}.vault.azure.net/", Configuration["DimeCertificateName"]).Result;

            X509Certificate2 certificate = new X509Certificate2(Convert.FromBase64String(certificateSecret.Value), String.Empty, X509KeyStorageFlags.MachineKeySet);

            return certificate;
        }

        private string GenerateJWT(X509Certificate2 certificate, string clientId)
        {
            // Get private key from certificate
            X509SecurityKey privateKey = new X509SecurityKey(certificate);

            // Extract public certificates
            var certificateList = new List<string>()
            {
                Convert.ToBase64String(certificate.RawData)
            };

            var now = DateTime.UtcNow;
            var tokenHandler = new JwtSecurityTokenHandler
            {
                SetDefaultTimesOnTokenCreation = false
            };

            var claims = new List<Claim>
            {
                new Claim("scope", "openid"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                IssuedAt = now,
                Expires = now.AddMinutes(2),
                Audience = "https://oidc-ver1.difi.no/idporten-oidc-provider/",
                Issuer = clientId,

                SigningCredentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256)
            };

            JwtSecurityToken stoken = (JwtSecurityToken)tokenHandler.CreateToken(tokenDescriptor);
            stoken.Header.Remove("x5t");
            stoken.Header.Remove("kid");
            stoken.Header.Add("x5c", new List<string>() { certificateList[0] });

            return tokenHandler.WriteToken(stoken);
        }
    }
}
