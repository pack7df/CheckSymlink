using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spt.Shared.Core.Models;
using Spt.Shared.Domain.Queries;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Spt.Shared.Store.EntityFramework.Queries
{
    public class EntityFrameworkQueryHandler<TProjection> : IRequestHandler<Query<TProjection>, QueryResult<TProjection>>
    {
        protected IDbContextFactory DbContextFactory { get; }

        public EntityFrameworkQueryHandler(IDbContextFactory dbContextFactory)
        {
            DbContextFactory = dbContextFactory;
        }

        public virtual async Task<QueryResult<TProjection>> Handle(Query<TProjection> query, CancellationToken cancellationToken)
        {
            if (query.EntityType == null)
                throw new ArgumentException($"Cannot execute query '{query.GetType().Name}' with '{GetType().Name}'");

            using (var context = DbContextFactory.GetContext())
            {
                var thisType = GetType();
                return await (Task<QueryResult<TProjection>>)thisType.GetMethod("ExecuteQueryableAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).MakeGenericMethod(new[] { query.EntityType }).Invoke(this, new object[] { context, query, null, null });
            }
        }

        protected virtual async Task<QueryResult<TProjection>> ExecuteQueryAsync<TEntity>(DbContext context, Query<TProjection> query, Func<IQueryable<TEntity>, IQueryable<TEntity>> filter = null, Expression<Func<TEntity, TProjection>> projection = null)
            where TEntity : class
        {
            var linqQuery = context.Set<TEntity>().AsNoTracking();
            if (query.Includes != null && query.Includes.Any())
            {
                foreach (var include in query.Includes)
                {
                    linqQuery = linqQuery.Include(include);
                }
            }
            if (query.Filter != null)
            {
                linqQuery = linqQuery.Where((Expression<Func<TEntity, bool>>)query.Filter);
            }
            if (filter != null)
            {
                linqQuery = filter(linqQuery);
            }
            if (query.OrderBy != null && query.OrderBy.Any())
            {
                var orderBy = query.OrderBy.First();
                IOrderedQueryable<TEntity> orderedQuery;

                if (orderBy.Ascending)
                {
                    orderedQuery = linqQuery.OrderBy((Expression<Func<TEntity, object>>)orderBy.Field);
                }
                else
                {
                    orderedQuery = linqQuery.OrderByDescending((Expression<Func<TEntity, object>>)orderBy.Field);
                }
                foreach (var thenBy in query.OrderBy.Skip(1))
                {
                    if (thenBy.Ascending)
                    {
                        orderedQuery = orderedQuery.ThenBy((Expression<Func<TEntity, object>>)thenBy.Field);
                    }
                    else
                    {
                        orderedQuery = orderedQuery.ThenByDescending((Expression<Func<TEntity, object>>)thenBy.Field);
                    }
                }
                linqQuery = orderedQuery;
            }

            var serverCount = await linqQuery.CountAsync().ConfigureAwait(false);

            linqQuery = linqQuery
               .Skip(query.Skip)
               .Take(query.Take);

            IQueryable<TProjection> result;
            if (projection != null)
                result = linqQuery.Select(projection);
            else if (query.Projection != null)
                result = linqQuery.Select((Expression<Func<TEntity, TProjection>>)query.Projection);
            else
                result = linqQuery.ProjectToType<TProjection>();

            return new QueryResult<TProjection>(await result.ToListAsync(), serverCount);
        }
    }
}
