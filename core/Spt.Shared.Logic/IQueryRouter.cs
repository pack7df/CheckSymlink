using Spt.Shared.Core.Models;
using Spt.Shared.Domain.Queries;
using System.Threading.Tasks;

namespace Spt.Shared.Logic
{
    public interface IQueryRouter
    {
        Task<QueryResult<TProjection>> QueryAsync<TProjection>(Query<TProjection> query);
        Task<TProjection> QueryOneAsync<TProjection>(Query<TProjection> query);
        Task<TState> QueryStateAsync<TProcessManager, TKey, TState>(TKey key);
    }
}
