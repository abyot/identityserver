using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace IdentityServer
{
    public class ProfileService : IProfileService
    {
        private readonly string[] _claimTypesToMap = { "name", "role", "upn", "unique_name", "family_name", "given_name", "pid", "groups" };

        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            foreach (var claimType in _claimTypesToMap)
            {
                var claims = context.Subject.Claims.Where(c => c.Type == claimType);
                context.IssuedClaims.AddRange(claims);
            }
            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
            return Task.FromResult(true);
        }
        
    }
}