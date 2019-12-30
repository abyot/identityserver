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
        private readonly MongoDbDataContext _kommuneDbContext = new MongoDbDataContext("TenantIdentity");
        private readonly MongoDbDataContext _personDbContext = new MongoDbDataContext("PersonLocation");

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            string Email = null;
            string Pid = null;

            foreach (var claimType in _claimTypesToMap)
            {
                var claims = context.Subject.Claims.Where(c => c.Type == claimType);

                foreach (var claim in claims)
                {
                    if (claim.Type.Equals("unique_name"))
                    {
                        Email = claim.Value;
                    }
                    else if(claim.Type.Equals("pid"))
                    {
                        Pid = claim.Value;
                    }
                }

                context.IssuedClaims.AddRange(claims);
            }

            if( Email != null )
            {
                FilterDefinition<Tenant> filter = Builders<Tenant>.Filter.Eq("Email", Email);

                Tenant loggedInTenant = await this._kommuneDbContext.Tenant.Find(filter).FirstOrDefaultAsync();

                if( loggedInTenant != null )
                {
                    var claims = new List<Claim>();

                    foreach (var k in loggedInTenant.Kommunenummerer)
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

            if ( Pid != null )
            {
                FilterDefinition<PersonLocation> filter = Builders<PersonLocation>.Filter.Eq("pid", Pid);

                PersonLocation loggedInPerson = await this._personDbContext.PersonLocation.Find(filter).FirstOrDefaultAsync();

                if (loggedInPerson !=null && loggedInPerson.kommunenummer > 0 )
                {
                    context.IssuedClaims.Add(new Claim("Kommunenummer", loggedInPerson.kommunenummer.ToString()));
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