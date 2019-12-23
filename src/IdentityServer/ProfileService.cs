using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer.Models;
using IdentityServer.MongoContext;
using IdentityServer4.Models;
using IdentityServer4.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace IdentityServer
{
    public class ProfileService : IProfileService
    {
        private readonly string[] _claimTypesToMap = { "name", "role", "upn", "unique_name", "pid", "groups" };
        private readonly MongoDbDataContext _dbContext = new MongoDbDataContext("TenantIdentity");

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            string Email = null;

            foreach (var claimType in _claimTypesToMap)
            {
                var claims = context.Subject.Claims.Where(c => c.Type == claimType);

                foreach (var claim in claims)
                {
                    if (claim.Type.Equals("unique_name"))
                    {
                        Email = claim.Value;
                    }
                }

                context.IssuedClaims.AddRange(claims);
            }

            if(Email != null)
            {
                FilterDefinition<Tenant> filter = Builders<Tenant>.Filter.Eq("Email", Email);
                IEnumerable<Tenant> tenants = null;
                using (IAsyncCursor<Tenant> cursor = await this._dbContext.GetTenants.FindAsync(new BsonDocument()))
                {
                    while (await cursor.MoveNextAsync())
                    {
                        tenants = cursor.Current;
                    }
                }

                if( tenants.Count() > 0 )
                {
                    Tenant loggedInTenant = tenants.FirstOrDefault();

                    var claims = new List<Claim>();

                    foreach ( var k in loggedInTenant.Kommunenummerer)
                    {
                        claims.Add(new Claim("Kommunenummerer", k.ToString()));
                    }

                    foreach (var o in loggedInTenant.Organisasjonsnummerer)
                    {
                        claims.Add(new Claim("Organisasjonsnummerer", o.ToString()));
                    }

                    claims.Add(new Claim("Kommunenummerer", ""));
                    claims.Add(new Claim("Organisasjonsnummerer", ""));

                    context.IssuedClaims.AddRange(claims);
                }
            }

            //return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
            return Task.FromResult(true);
        }
        
    }
}