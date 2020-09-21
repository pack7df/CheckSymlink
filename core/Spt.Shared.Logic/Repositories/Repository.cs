using System;
using System.Threading.Tasks;
using Spt.Shared.Domain.Repositories;
using Spt.Shared.Domain.Aggregates;
using Spt.Shared.Core.Services;

namespace Spt.Shared.Logic.Repositories
{
    public abstract class Repository<TKey, TAggregateRoot> : IRepository<TKey, TAggregateRoot> where TAggregateRoot : AggregateRoot<TKey>
    {
        protected Func<string, string> t { get; set; }

        public Repository()
        {
            var textTranslator = Core.ApplicationContext.ServiceProvider.GetService<ITextTranslator>();
            t = textTranslator == null ? (Func<string, string>)(o => o) : textTranslator.Translate;
        }

        public abstract Task DeleteAsync(TAggregateRoot aggregate);

        public abstract Task<TAggregateRoot> GetByIdAsync(TKey id);

        public abstract Task SaveAsync(TAggregateRoot aggregate);
    }
}
