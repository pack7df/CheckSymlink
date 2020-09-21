using System;
using System.Threading.Tasks;

namespace Spt.Shared.Logic.State
{
    public interface IStateManager
    {
        Task<TState> GetStateAsync<TKey, TState>(TKey key);

        Task<TState> SetStateAsync<TKey, TState>(TKey key, Func<TState, TState> setState);
    }
}
