using Spt.Shared.Logic.State;
using System;
using System.Threading.Tasks;

namespace Spt.Shared.Logic.Processes
{
    public abstract class ProcessManager
    {
        protected ICommandRouter CommandRouter { get; }

        public ProcessManager()
        {
            CommandRouter = Core.ApplicationContext.ServiceProvider.GetService<ICommandRouter>();
        }
    }

    public abstract class ProcessManager<TKey, TState> : ProcessManager
    {
        private readonly IStateManager _stateManager;

        public ProcessManager() : base()
        {
            _stateManager = Core.ApplicationContext.ServiceProvider.GetService<IStateManager>();
        }

        protected async Task<TState> GetStateAsync(TKey key)
        {
            return await _stateManager.GetStateAsync<string, TState>(GetStateKey(key)).ConfigureAwait(false);
        }

        protected async Task<TState> SetStateAsync(TKey key, Func<TState, TState> setState)
        {
            return await _stateManager.SetStateAsync(GetStateKey(key), setState).ConfigureAwait(false);
        }

        private string GetStateKey(TKey key)
        {
            return GetStateKey(GetType().Name, key);
        }

        internal static string GetStateKey(string typeName, TKey key)
        {
            return $"{typeName}_{key}".TrimEnd(new char[] { '_' });
        }
    }
}
