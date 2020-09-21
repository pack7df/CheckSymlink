using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Spt.Shared.Core;
using Spt.Shared.Core.Models;
using Spt.Shared.Domain.Queries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapster;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Spt.Shared.Store.EntityFramework
{
    public static class Extensions
    {
        public static async Task<QueryResult<TProjection>> ExecuteQueryAsync<TEntity, TProjection>(this IQueryable<TEntity> linqQuery, Query<TProjection> query = null)
            where TEntity : class
        {
            if (query == null)
            {
                query = new Query<TEntity, TProjection>();
            }
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
            if (query.Projection != null)
                result = linqQuery.Select((Expression<Func<TEntity, TProjection>>)query.Projection);
            else
                result = linqQuery.ProjectToType<TProjection>();

            return new QueryResult<TProjection>(await result.ToListAsync(), serverCount);
        }

        public static IQueryable<T> IncludeAll<T>(this DbSet<T> dbSet) where T : class
        {
            var infrastructure = dbSet as IInfrastructure<IServiceProvider>;
            var serviceProvider = infrastructure.Instance;
            var currentDbContext = serviceProvider.GetService(typeof(ICurrentDbContext)) as ICurrentDbContext;
            var navigationPropertyPaths = currentDbContext.Context.GetIncludePaths(typeof(T));
            return navigationPropertyPaths.Aggregate((IQueryable<T>)dbSet, (query, path) => query.Include(path));
        }

        public static string[] GetIncludePaths(this DbContext context, Type clrEntityType)
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

        public static void CopyTo<T>(this T source, T target, DbContext context)
        {
            var sourceProperties = source.GetType().GetProperties();
            var targetProperties = target.GetType().GetProperties();
            foreach (var property in sourceProperties)
            {
                if (ObjectExtensions.IsPrimitive(property.PropertyType) || property.PropertyType.Name == "Nullable`1" || property.PropertyType.Name == "JObject")
                {
                    var getMethod = property.GetGetMethod();
                    var setMethod = targetProperties.FirstOrDefault(p => p.Name == property.Name).GetSetMethod(true) ?? property.GetSetMethod(true);
                    var sourceValue = getMethod.Invoke(source, new object[0]);
                    var destValue = getMethod.Invoke(target, new object[0]);
                    if (setMethod != null && ((sourceValue == null && destValue != null) || (sourceValue != null && destValue == null) || (sourceValue != null && !sourceValue.Equals(destValue))))
                    {
                        setMethod.Invoke(target, new[] { sourceValue });
                        //var state = context.Entry(target).State;
                        //if (state == EntityState.Unchanged || state == EntityState.Detached)
                        //{
                        //    context.Entry(target).State = EntityState.Modified;
                        //}
                    }
                }
                else
                {
                    if (property.PropertyType.Name == "IReadOnlyCollection`1" || property.PropertyType.Name == "IReadOnlyList`1")
                    {
                        var sourceField = source.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(f => f.Name.Equals($"_{property.Name}", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                        if (sourceField != null)
                        {
                            var targetField = target.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(f => f.Name.Equals($"_{property.Name}", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                            var targetValue = targetField.GetValue(target);
                            var parameters = new[] { sourceField.GetValue(source), targetValue, context };
                            var mergeMethod = typeof(Extensions).GetMethod("MergeCollections", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(new Type[] { property.PropertyType.GetGenericArguments()[0] });
                            mergeMethod.Invoke(null, parameters);
                            if (targetValue == null && parameters[1] != null)
                            {
                                targetField.SetValue(target, parameters[1]);
                            }
                        }
                    }
                    if (property.PropertyType.Name == "ICollection`1" || property.PropertyType.Name == "IList`1")
                    {
                        var getMethod = property.GetGetMethod();
                        var targetGetMethod = targetProperties.FirstOrDefault(p => p.Name == property.Name).GetGetMethod();
                        var mergeMethod = typeof(Extensions).GetMethod("MergeCollections", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(new Type[] { property.PropertyType.GetGenericArguments()[0] });
                        var targetValue = targetGetMethod.Invoke(target, new object[0]);
                        var parameters = new[] { getMethod.Invoke(source, new object[0]), targetValue, context };
                        mergeMethod.Invoke(null, parameters);
                        if (targetValue == null && parameters[1] != null)
                        {
                            var setMethod = targetProperties.FirstOrDefault(p => p.Name == property.Name).GetSetMethod(true);
                            setMethod.Invoke(target, new[] { parameters[1] });
                        }
                    }
                }
            }
        }

        private static void MergeCollections<TEntity>(ICollection<TEntity> source, ref ICollection<TEntity> target, DbContext context)
        {
            var type = typeof(TEntity);
            if (source == null)
            {
                target = null;
                return;
            }

            if (target == null)
            {
                target = new Collection<TEntity>();
            }

            // Delete non-existing in target
            foreach (var item in target.ToList().Where(t => source.All(s => s == null || !s.Equals(t))))
            {
                target.Remove(item);
                context.Entry(item).State = EntityState.Deleted;
            }

            // Merge existing items or add new ones
            foreach (var s in source)
            {
                var t = target.FirstOrDefault(o => o.Equals(s));
                if (t == null)
                {
                    var newEntry = (TEntity)FormatterServices.GetUninitializedObject(type);
                    s.CopyTo(newEntry, context);
                    target.Add(newEntry);
                    context.Entry(newEntry).State = EntityState.Added;
                }
                else
                {
                    var sType = s.GetType();
                    //// Check if there is a hard delete attribute
                    //if (sType.GetCustomAttributes(typeof(HardDeleteAttribute), true).Length > 0)
                    //{
                    //    var sIsDelete = sType.GetProperty("IsDeleted")?.GetGetMethod();
                    //    if (sIsDelete != null && (bool)sIsDelete.Invoke(s, new object[0]))
                    //    {
                    //        // Remove it since it is HardDelete and IsDeleted = true
                    //        target.Remove(t);
                    //    }
                    //    else
                    //    {
                    //        // Recursive update properties
                    //        CopyTo(s, t);
                    //    }
                    //}
                    //else
                    //{
                    // Recursive update properties
                    CopyTo(s, t, context);
                    //}
                }
            }
        }
    }
}
