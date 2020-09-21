using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Spt.Shared.Core
{
    public interface IServiceProviderProxy
    {
        T GetService<T>();
        IEnumerable<T> GetServices<T>();
        object GetService(Type type);
        IEnumerable<object> GetServices(Type type);
        ClaimsIdentity GetCurrentIdentity();
    }
}
