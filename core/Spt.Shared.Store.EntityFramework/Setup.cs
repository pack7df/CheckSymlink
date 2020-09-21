using Microsoft.Extensions.DependencyInjection;
using Spt.Shared.Core.Attributes;
using Spt.Shared.Domain.Aggregates;
using Spt.Shared.Domain.Repositories;
using Spt.Shared.Logic;
using Spt.Shared.Store.EntityFramework.Queries;
using Spt.Shared.Store.EntityFramework.Repositories;
using System.Linq;
using System.Reflection;

namespace Spt.Shared.Store.EntityFramework
{
    public static class Setup
    {
        public static void AddEntityFramework<TDbContextFactory>(this IServiceCollection services, params Assembly[] assemblies) where TDbContextFactory : class, IDbContextFactory
        {
            services.AddSingleton<IDbContextFactory, TDbContextFactory>();

            if (assemblies != null)
            {
                foreach (var assembly in assemblies)
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.BaseType.Name == "AggregateRoot`1" && !type.IsAbstract)
                        {
                            var keyType = type.BaseType.GetGenericArguments()[0];
                            var @interface = typeof(IRepository<,>).MakeGenericType(new[] { keyType, type });
                            var service = typeof(EntityFrameworkRepository<,>).MakeGenericType(new[] { keyType, type });
                            services.AddSingleton(@interface, service);
                        }

                        if (type.BaseType.Name == "EntityFrameworkQueryHandler`1" && !type.IsAbstract)
                        {
                            services.AddSingleton(type);
                            Registrar.QueryHandlers.Add(type.BaseType.GetGenericArguments().First().FullName, type);
                        }

                        if (type.GetCustomAttribute<QueryResultAttribute>(true) != null)
                        {
                            var queryHandlerType = typeof(EntityFrameworkQueryHandler<>).MakeGenericType(new[] { type });
                            services.AddSingleton(queryHandlerType);
                            Registrar.QueryHandlers.Add(type.FullName, queryHandlerType);
                        }
                    }
                }
            }
        }

        public static void AddEntityFrameworkRepository<TKey, TAggregate>(this IServiceCollection services) where TAggregate : AggregateRoot<TKey>
        {
            var @interface = typeof(IRepository<,>).MakeGenericType(new[] { typeof(TKey), typeof(TAggregate) });
            var service = typeof(EntityFrameworkRepository<,>).MakeGenericType(new[] { typeof(TKey), typeof(TAggregate) });
            services.AddSingleton(@interface, service);
        }
    }
}
