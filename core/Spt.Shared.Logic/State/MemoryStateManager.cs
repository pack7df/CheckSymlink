using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spt.Shared.Logic.State
{
    public class MemoryStateManager : IStateManager
    {
        private object _lock = new object();
        private readonly Dictionary<string, object> _state = new Dictionary<string, object>();

        public Task<TState> GetStateAsync<TKey, TState>(TKey key)
        {
            lock (_lock)
            {
                if (_state.TryGetValue(key.ToString(), out var item))
                    return Task.FromResult((TState)item);
                return Task.FromResult(default(TState));
            }
        }

        public Task<TState> SetStateAsync<TKey, TState>(TKey key, Func<TState, TState> setState)
        {
            lock (_lock)
            {
                _state.TryGetValue(key.ToString(), out var item);
                var result = setState((TState)item);
                _state[key.ToString()] = result;
                return Task.FromResult(result);
            }
        }
    }
}
