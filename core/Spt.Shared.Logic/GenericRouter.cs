using MediatR;
using Spt.Shared.Core.Commands;
using Spt.Shared.Core.Events;
using Spt.Shared.Core.Models;
using Spt.Shared.Domain.Queries;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Spt.Shared.Logic
{
    internal class GenericRouter : ICommandRouter, IEventRouter, IQueryRouter
    {
        private readonly IMediator _mediator;
        private readonly IServiceProvider _serviceProvider;

        public GenericRouter(IMediator mediator, IServiceProvider serviceProvider)
        {
            _mediator = mediator;
            _serviceProvider = serviceProvider;
        }

        public async Task<TResponse> ExecuteAsync<TResponse>(Command<TResponse> command)
        {
            return await _mediator.Send(command).ConfigureAwait(false);
        }

        public async Task<TResponse> ExecuteAsync<TRequest, TResponse>(Command<TRequest, TResponse> command)
        {
            return await _mediator.Send<TResponse>(command).ConfigureAwait(false);
        }

        public async Task<QueryResult<TProjection>> QueryAsync<TProjection>(Query<TProjection> query)
        {
            if (Registrar.QueryHandlers.TryGetValue(typeof(TProjection).FullName, out var handlerType))
            {
                return await ((Task<QueryResult<TProjection>>)handlerType.GetMethod("Handle").Invoke(_serviceProvider.GetService(handlerType), new object[] { query, CancellationToken.None })).ConfigureAwait(false);
            }
            throw new ArgumentException($"There is not query handler for '{typeof(TProjection).FullName}'");
            //return await _mediator.Send<QueryResult<TProjection>>(query).ConfigureAwait(false);
        }

        public async Task<TProjection> QueryOneAsync<TProjection>(Query<TProjection> query)
        {
            query.SetTake(2);
            var result = await QueryAsync(query);
            if (result.Count > 1)
                throw new ArgumentException("Multiple entities where retrieved in the QueryOneAsync method. This seems to be a BUG in the query definition");
            return result.Items.FirstOrDefault();
        }

        public Task<TState> QueryStateAsync<TProcessManager, TKey, TState>(TKey key)
        {
            throw new NotImplementedException();
        }

        public Task SendAsync<T>(Command<T> command)
        {
            var task = _mediator.Send(command).ConfigureAwait(false);
            return Task.CompletedTask;
        }

        public Task SendAsync<TRequest, TResponse>(Command<TRequest, TResponse> command)
        {
            var task = _mediator.Send(command).ConfigureAwait(false);
            return Task.CompletedTask;
        }

        public async Task SendAsync<T>(T @event) where T : Event
        {
            await _mediator.Publish(@event).ConfigureAwait(false);
        }
    }
}
