using System.Security.Claims;

namespace Spt.Shared.Core.Services
{
    public interface ICurrentUserAccessor
    {
        ClaimsIdentity Identity { get; set; }
    }
}
