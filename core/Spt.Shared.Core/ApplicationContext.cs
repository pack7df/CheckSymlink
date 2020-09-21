using System;
using System.Security.Claims;

namespace Spt.Shared.Core
{
    public static class ApplicationContext
    {
        private static IServiceProviderProxy _serviceProvider;

        public static ClaimsIdentity CurrentUserIdentity => _serviceProvider?.GetCurrentIdentity();

        public static IServiceProviderProxy ServiceProvider => _serviceProvider ?? throw new Exception("You should Initialize the ServiceProvider before using it.");

        public static void Initialize(IServiceProviderProxy proxy)
        {
            _serviceProvider = proxy;
        }
    }
}
