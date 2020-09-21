using MediatR;
using Spt.Shared.Core.Models;
using Spt.Shared.Domain.Converters;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace Spt.Shared.Domain.Queries
{
    [DataContract]
    [Serializable]
    public class Query<TProjection> : IRequest<QueryResult<TProjection>>
    {
        [DataMember(Name = "Includes")]
        protected List<string> _includes;

        [DataMember(Name = "EntityType")]
        protected string _entityType;

        [DataMember(Name = "Parameters")]
        protected Dictionary<string, object> _parameters;

        [DataMember]
        public int Skip { get; protected set; }

        [DataMember]
        public int Take { get; protected set; }

        [DataMember(Name = "OrderBy")]
        protected List<OrderBy> _orderBy;

        [DataMember(Name = "Filter")]
        protected byte[] _filter;

        [DataMember(Name = "Projection")]
        protected byte[] _projection;

        [IgnoreDataMember]
        public Type EntityType
        {
            get
            {
                if (_entityType == null)
                    return null;
                return Type.GetType(_entityType);
            }
            protected set
            {
                _entityType = value.AssemblyQualifiedName;
            }
        }

        [IgnoreDataMember]
        public Expression Filter
        {
            get
            {
                if (_filter == null)
                    return null;
                return BinaryLinqExpressionConverter.ReadExpression(_filter);
            }
            protected set
            {
                _filter = BinaryLinqExpressionConverter.WriteExpression(value);
            }
        }

        [IgnoreDataMember]
        public Expression Projection
        {
            get
            {
                if (_projection == null)
                    return null;
                return BinaryLinqExpressionConverter.ReadExpression(_projection);
            }
            protected set
            {
                _projection = BinaryLinqExpressionConverter.WriteExpression(value);
            }
        }

        [IgnoreDataMember]
        public IReadOnlyList<string> Includes => _includes;

        [IgnoreDataMember]
        public IReadOnlyList<OrderBy> OrderBy => _orderBy;

        [IgnoreDataMember]
        public IReadOnlyDictionary<string, object> Parameters => _parameters;

        public Query()
        {
            Take = 5000;
        }

        public Query<TProjection> SetSkip(int value)
        {
            Skip = value;
            return this;
        }

        public Query<TProjection> SetTake(int value)
        {
            Take = value;
            return this;
        }

        public Query<TProjection> AddParameter(string name, object value)
        {
            if (_parameters == null)
                _parameters = new Dictionary<string, object>();
            _parameters.Add(name, value);
            return this;
        }

        public Query<TProjection> AddInclude(string include)
        {
            if (_includes == null)
                _includes = new List<string>();
            _includes.Add(include);
            return this;
        }

        public Query<TProjection> AddIncludes(params string[] includes)
        {
            foreach (var include in includes)
            {
                AddInclude(include);
            }
            return this;
        }
    }

    [DataContract]
    [Serializable]
    public class Query<TEntity, TProjection> : Query<TProjection>
        where TEntity : class
    {
        internal class ExpressionParaneterRenamer : ExpressionVisitor
        {
            private readonly string _sourceName;
            private readonly string _destName;

            public ExpressionParaneterRenamer(string sourceName, string destName)
            {
                _sourceName = sourceName;
                _destName = destName;
            }
            public Expression Rename(Expression expression)
            {
                return Visit(expression);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node.Name == _sourceName)
                    return Expression.Parameter(node.Type, _destName);
                else
                    return node;
            }
        }

        public Query() : base()
        {
            EntityType = typeof(TEntity);
        }

        public new Query<TEntity, TProjection> AddParameter(string name, object value)
        {
            return (Query<TEntity, TProjection>)base.AddParameter(name, value);
        }

        public new Query<TEntity, TProjection> AddInclude(string include)
        {
            return (Query<TEntity, TProjection>)base.AddInclude(include);
        }

        public new Query<TEntity, TProjection> AddIncludes(params string[] includes)
        {
            return (Query<TEntity, TProjection>)base.AddIncludes(includes);
        }

        public Query<TEntity, TProjection> AddFilter(Expression<Func<TEntity, bool>> filter)
        {
            if (Filter == null)
            {
                Filter = filter;
                return this;
            }
            var filterExpression = (Expression<Func<TEntity, bool>>)Filter;
            var parameter = Expression.Parameter(typeof(TEntity), filterExpression.Parameters[0].Name);

            var newFilterExpression = new ExpressionParaneterRenamer(filter.Parameters[0].Name, filterExpression.Parameters[0].Name).Rename(filter.Body);
            var body = Expression.And(filterExpression.Body, newFilterExpression);
            LambdaExpression lambda = Expression.Lambda(body, parameter);
            Filter = (Expression<Func<TEntity, bool>>)lambda;
            return this;
        }

        public new Query<TEntity, TProjection> SetSkip(int value)
        {
            return (Query<TEntity, TProjection>)base.SetSkip(value);
        }

        public new Query<TEntity, TProjection> SetTake(int value)
        {
            return (Query<TEntity, TProjection>)base.SetTake(value);
        }

        public Query<TEntity, TProjection> SetProjection(Expression<Func<TEntity, TProjection>> projection)
        {
            Projection = projection;
            return this;
        }

        public Query<TEntity, TProjection> AddOrderBy(Expression<Func<TEntity, object>> orderBy, bool ascending = true)
        {
            if (_orderBy == null)
                _orderBy = new List<OrderBy>();
            _orderBy.Add(new OrderBy(orderBy, ascending));
            return this;
        }
    }

    [DataContract]
    [Serializable]
    public class OrderBy
    {
        [DataMember(Name = "OrderByField")]
        private byte[] _field;

        [IgnoreDataMember]
        public Expression Field
        {
            get
            {
                if (_field == null)
                    return null;
                return BinaryLinqExpressionConverter.ReadExpression(_field);
            }
            private set
            {
                _field = BinaryLinqExpressionConverter.WriteExpression(value);
            }
        }

        [DataMember]
        public bool Ascending { get; private set; }

        public OrderBy(Expression field, bool ascending = true)
        {
            Field = field;
            Ascending = ascending;
        }
    }
}
