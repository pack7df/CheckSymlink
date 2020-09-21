using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Spt.Shared.Core;
using Spt.Shared.Core.Services;

namespace Spt.Shared.AspNetCore
{
    public static class Setup
    {
        public static void AddServiceLocator(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<IServiceProviderProxy, HttpContextServiceProviderProxy>();
            services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        }

        public static void UseServiceLocator(this IApplicationBuilder app)
        {
            ApplicationContext.Initialize(app.ApplicationServices.GetService<IServiceProviderProxy>());
        }
    }
}
