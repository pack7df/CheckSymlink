using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Spt.Shared.Domain.Aggregates;
using Spt.Shared.Logic.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spt.Shared.Store.EntityFramework.Repositories
{
    public class EntityFrameworkRepository<TKey, TAggregate> : Repository<TKey, TAggregate> where TAggregate : AggregateRoot<TKey>
    {
        private static ConcurrentDictionary<string, string[]> _navigationPaths = new ConcurrentDictionary<string, string[]>();
        private readonly IDbContextFactory _contextFactory;

        public EntityFrameworkRepository(IDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public override async Task<TAggregate> GetByIdAsync(TKey id)
        {
            // TODO: Patch aggregate on all entities
            var type = typeof(TAggregate);
            using (var ctx = _contextFactory.GetContext())
            {
                var paths = _navigationPaths.GetOrAdd(type.Name, typeName => GetIncludePaths(ctx, typeof(TAggregate)));
                return await Include(ctx.Set<TAggregate>().AsNoTracking(), paths).FirstOrDefaultAsync(o => o.Id.Equals(id)).ConfigureAwait(false);
            }
        }

        public override async Task SaveAsync(TAggregate aggregate)
        {
            if (aggregate == null)
                throw new ArgumentException($"Aggregate cannot be null on {nameof(SaveAsync)} method of {GetType().Name}");
            using (var ctx = _contextFactory.GetContext())
            {
                if (ctx.ChangeTracker.Entries<TAggregate>().All(o => !o.Entity.Equals(aggregate)))
                    await AttachEntityAsync(ctx, aggregate).ConfigureAwait(false);
                await ctx.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public override async Task DeleteAsync(TAggregate aggregate)
        {
            if (aggregate == null)
                throw new ArgumentException($"Aggregate cannot be null on {nameof(DeleteAsync)} method of {GetType().Name}");
            using (var ctx = _contextFactory.GetContext())
            {
                var entity = ctx.Find<TAggregate>(aggregate.Id);
                if (entity == null)
                    throw new ArgumentException($"Entity {typeof(TAggregate).Name} with id '{aggregate.Id}' not found.");
                ctx.Set<TAggregate>().Remove(entity);
                await ctx.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private async Task AttachEntityAsync(DbContext ctx, TAggregate aggregate)
        {
            var type = typeof(TAggregate);
            var paths = _navigationPaths.GetOrAdd(type.Name, typeName => GetIncludePaths(ctx, typeof(TAggregate)));
            var entity = await Include(ctx.Set<TAggregate>(), paths).FirstOrDefaultAsync(o => o.Id.Equals(aggregate.Id)).ConfigureAwait(false);
            if (entity == null)
            {
                ctx.Attach(aggregate).State = EntityState.Added;
            }
            else
            {
                aggregate.CopyTo(entity, ctx);
            }
        }

        private IQueryable<T> Include<T>(IQueryable<T> source, IEnumerable<string> navigationPropertyPaths)
            where T : class
        {
            return navigationPropertyPaths.Aggregate(source, (query, path) => query.Include(path));
        }

        private static string[] GetIncludePaths(DbContext context, Type clrEntityType)
        {
            var entityType = context.Model.FindEntityType(clrEntityType);
            var includedNavigations = new HashSet<INavigation>();
            var stack = new Stack<IEnumerator<INavigation>>();
            var result = new List<string>();
            while (true)
            {
                var entityNavigations = new List<INavigation>();
                foreach (var navigation in entityType.GetNavigations())
                {
                    if (includedNavigations.Add(navigation))
                        entityNavigations.Add(navigation);
                }
                if (entityNavigations.Count == 0)
                {
                    if (stack.Count > 0)
                        result.Add(string.Join(".", stack.Reverse().Select(e => e.Current.Name)));
                }
                else
                {
                    foreach (var navigation in entityNavigations)
                    {
                        var inverseNavigation = navigation.FindInverse();
                        if (inverseNavigation != null)
                            includedNavigations.Add(inverseNavigation);
                    }
                    stack.Push(entityNavigations.GetEnumerator());
                }
                while (stack.Count > 0 && !stack.Peek().MoveNext())
                    stack.Pop();
                if (stack.Count == 0) break;
                entityType = stack.Peek().Current.GetTargetType();
            }
            return result.ToArray();
        }
    }
}
