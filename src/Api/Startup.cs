using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Api
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore()
                .AddAuthorization(options => options.AddPolicy("BHGAdmin", policy => policy.RequireClaim("Groups", "723c8f87-0888-4a66-909f-21a63bd6fc94")))
                .AddJsonFormatters();

            services
                .AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options => {
                    options.Authority = "https://dimeidentityserver.azurewebsites.net";
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidAudiences = new List<string> 
                        {
                            "bhgapp",
                            "bhgadmin"
                        }
                    };
                });

            services
                .AddCors(options => {
                    options.AddPolicy("default", policy =>
                    {
                        policy
                        .WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseCors("default");

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
