using System;
using IdentityServer4;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;

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
            
            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            var builder = services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApis())
                .AddInMemoryClients(Config.GetClients(Configuration))
                .AddTestUsers(Config.GetUsers())
                .AddProfileService<ProfileService>();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication()
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

            app.UseIdentityServer();

            app.UseMvcWithDefaultRoute();
        }
    }
}
