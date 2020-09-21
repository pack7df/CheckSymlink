using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Spt.Shared.Core;

namespace Spt.Shared.AspNetCore
{
    public class HttpContextServiceProviderProxy : IServiceProviderProxy
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _contextAccessor;

        public HttpContextServiceProviderProxy(IServiceProvider serviceProvider, IHttpContextAccessor contextAccessor)
        {
            _serviceProvider = serviceProvider;
            _contextAccessor = contextAccessor;
        }

        public T GetService<T>()
        {
            var httpContext = _contextAccessor.HttpContext;
            if (httpContext == null)
                _serviceProvider.GetService<T>();
            return httpContext.RequestServices.GetService<T>();
        }

        public IEnumerable<T> GetServices<T>()
        {
            var httpContext = _contextAccessor.HttpContext;
            if (httpContext == null)
                _serviceProvider.GetServices<T>();
            return httpContext.RequestServices.GetServices<T>();
        }

        public object GetService(Type type)
        {
            var httpContext = _contextAccessor.HttpContext;
            if (httpContext == null)
                _serviceProvider.GetService(type);
            return httpContext.RequestServices.GetService(type);
        }

        public IEnumerable<object> GetServices(Type type)
        {
            var httpContext = _contextAccessor.HttpContext;
            if (httpContext == null)
                _serviceProvider.GetServices(type);
            return httpContext.RequestServices.GetServices(type);
        }

        public ClaimsIdentity GetCurrentIdentity()
        {
            var httpContext = _contextAccessor.HttpContext;
            if (httpContext == null)
                return null;
            return httpContext.User.Identity as ClaimsIdentity;
        }
    }
}
