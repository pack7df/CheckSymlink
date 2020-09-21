using Spt.Shared.Domain.Aggregates;
using System.Threading.Tasks;

namespace Spt.Shared.Domain.Repositories
{
    public interface IRepository<TKey, TAggregateRoot> where TAggregateRoot : AggregateRoot<TKey>
    {
        /// <summary>
        /// Saves the specified aggregate
        /// </summary>
        /// <param name="aggregate">The aggregate</param>
        /// <returns></returns>
        Task SaveAsync(TAggregateRoot aggregate);

        /// <summary>
        /// Gets the specified aggregate by Id
        /// </summary>
        /// <param name="id">The aggregate Identifier</param>
        /// <returns></returns>
        Task<TAggregateRoot> GetByIdAsync(TKey id);

        /// <summary>
        /// Delete the specified aggregate by Id
        /// </summary>
        /// <param name="aggregate">The aggregate you want to delete</param>
        /// <returns></returns>
        Task DeleteAsync(TAggregateRoot aggregate);
    }
}
