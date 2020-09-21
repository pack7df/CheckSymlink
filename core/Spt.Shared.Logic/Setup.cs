using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Spt.Shared.Core.Models;
using Spt.Shared.Domain.Queries;
using Spt.Shared.Logic.State;
using System;
using System.Linq;
using System.Reflection;

namespace Spt.Shared.Logic
{
    public static class Setup
    {
        private static GenericRouter _memoizedRouter;

        /// <summary>
        /// Use this for development only since this is NOT scalable, doesnt persist data and will not work in a cluster or web farm
        /// </summary>
        /// <param name="services"></param>
        [Obsolete("Use this for development only since this is NOT scalable, doesnt persist data and will not work in a cluster or web farm")]
        public static void AddInMemoryStateManager(this IServiceCollection services)
        {
            services.AddSingleton<IStateManager, MemoryStateManager>();
        }

        public static void AddQuery<TProjection, TQueryHandler>(this IServiceCollection services) where TQueryHandler : IRequestHandler<Query<TProjection>, QueryResult<TProjection>>
        {
            services.AddSingleton(typeof(TQueryHandler));
            Registrar.QueryHandlers.Add(typeof(TProjection).FullName, typeof(TQueryHandler));
        }

        public static void AddInMemoryRouter(this IServiceCollection services)
        {
            services.AddSingleton<ICommandRouter>(provider => _memoizedRouter ?? (_memoizedRouter = new GenericRouter(provider.GetService<IMediator>(), provider)));
            services.AddSingleton<IEventRouter>(provider => _memoizedRouter ?? (_memoizedRouter = new GenericRouter(provider.GetService<IMediator>(), provider)));
            services.AddSingleton<IQueryRouter>(provider => _memoizedRouter ?? (_memoizedRouter = new GenericRouter(provider.GetService<IMediator>(), provider)));
        }

        public static void AddDependenciesFrom(this IServiceCollection services, params Assembly[] assemblies)
        {

            if (assemblies != null)
            {
                foreach (var assembly in assemblies)
                {
                    services.AddMediatR(assembly);
                    foreach (var type in assembly.GetTypes())
                    {
                        var interfaces = type.GetInterfaces();
                        if (interfaces.Any(o => o.Name == "IQueryProcessor`1"))
                        {
                            // TODO
                        }
                    }
                }
            }
        }
    }
}
