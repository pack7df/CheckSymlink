using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace Spt.Shared.AspNetCore
{
    public static class IdentityExtensions
    {
        public static string GetDisplayName(this IPrincipal principal)
        {
            var claims = (principal as ClaimsPrincipal).Claims;
            var name = new List<string>();

            var firstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName);
            if (firstName != null)
                name.Add(firstName.Value);

            var lastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname);
            if (lastName != null)
                name.Add(lastName.Value);

            if (name.Count == 0)
            {
                var nameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                if (nameClaim != null)
                    name.Add(nameClaim.Value);
            }

            if (name.Count == 0)
            {
                var sub = claims.FirstOrDefault(c => c.Type == "sub");
                if (sub != null)
                    name.Add(sub.Value);
            }

            return string.Join(" ", name);
        }
    }
}
