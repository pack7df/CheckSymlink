using Microsoft.AspNet.OData.Query;
using Spt.Shared.Domain.Queries;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Spt.Shared.AspNetCore
{
    public static class ODataExtensions
    {
        internal static Expression<Func<TElement, bool>> ToExpression<TElement>(this FilterQueryOption filter)
        {
            if (filter == null)
                return null;

            // Avoid InvalidOperationException
            // see https://github.com/aspnet/EntityFrameworkCore/issues/10721
            var oDataSettings = new ODataQuerySettings();
            oDataSettings.HandleNullPropagation = HandleNullPropagationOption.False;

            var queryable = Enumerable.Empty<TElement>().AsQueryable();
            var where = filter.ApplyTo(queryable, oDataSettings).Expression;
            var whereType = where.GetType();
            var whereArguments = (ReadOnlyCollection<Expression>)whereType.GetProperty("Arguments").GetGetMethod().Invoke(where, new object[0]);
            var unaryExpression = whereArguments[1];
            var result = unaryExpression.GetType().GetProperty("Operand")?.GetGetMethod()?.Invoke(unaryExpression, new object[0]);
            return (Expression<Func<TElement, bool>>)result;
        }

        public static Query<TEntity, TProjection> AsQuery<TEntity, TProjection>(this ODataQueryOptions<TEntity> options, Func<Query<TEntity, TProjection>,
            Query<TEntity, TProjection>> extendQuery = null,
            Func<string, Expression<Func<TEntity, bool>>> buildSearch = null/*,
            Action<SortProfile, Query<TEntity, TProjection>> buildSortProfile = null*/)
            where TEntity : class
        {
            var query = new Query<TEntity, TProjection>();
            if (extendQuery != null)
            {
                query = extendQuery(query);
            }
            if (options != null)
            {
                if (options.Skip != null)
                {
                    query.SetSkip(options.Skip.Value);
                }
                if (options.Top != null)
                {
                    query.SetTake(options.Top.Value);
                }
                if (options.Filter != null)
                {
                    query.AddFilter(options.Filter.ToExpression<TEntity>());
                }
                if (options.Request.Query.ContainsKey("$search"))
                {
                    if (buildSearch == null)
                    {
                        buildSearch = q => model => true;
                    }
                    var searchFilter = buildSearch(options.Request.Query["$search"]);
                    if (searchFilter != null)
                    {
                        query.AddFilter(searchFilter);
                    }
                }
                //if (options.Request.Query.ContainsKey("sortProfile"))
                //{
                //    if (buildSortProfile == null)
                //    {
                //        buildSortProfile = (sp, qry) => { };
                //    }
                //    var raw = ((string)options.Request.Query["sortProfile"]).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                //    var sortProfile = new SortProfile(raw[0], raw[1]);
                //    buildSortProfile(sortProfile, query);
                //}
                //else
                {
                    if (options.OrderBy != null && options.OrderBy.OrderByClause != null)
                    {
                        var entityProps = typeof(TEntity).GetProperties().ToList();
                        foreach (var node in options.OrderBy.OrderByNodes)
                        {
                            var typedNode = node as OrderByPropertyNode;

                            var prop = entityProps.FirstOrDefault(o => o.Name.ToLowerInvariant() == typedNode.Property.Name.ToLowerInvariant());
                            if (prop == null)
                                throw new ArgumentException($"Property '{typedNode.Property.Name}' not found on entity '{typeof(TEntity).Name}'");

                            var param = Expression.Parameter(typeof(TEntity), "o");
                            var expression = Expression.Property(param, prop.Name);
                            Expression converted = Expression.Convert(expression, typeof(object));
                            dynamic lambda = Expression.Lambda<Func<TEntity, object>>(converted, param);

                            query.AddOrderBy(lambda, typedNode.OrderByClause.Direction == Microsoft.OData.UriParser.OrderByDirection.Ascending ? true : false);
                        }
                    }
                }
            }
            return query;
        }
    }
}
