using Microsoft.AspNetCore.Http;
using Spt.Shared.Core.Services;
using System.Security.Claims;

namespace Spt.Shared.AspNetCore
{
    public class CurrentUserAccessor : ICurrentUserAccessor
    {
        private ClaimsIdentity _identity;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimsIdentity Identity { get => _identity ?? (_identity = _httpContextAccessor.HttpContext?.User?.Identity as ClaimsIdentity); set => _identity = value; }

        public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
    }
}
