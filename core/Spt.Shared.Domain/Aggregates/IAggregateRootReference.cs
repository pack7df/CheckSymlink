using Spt.Shared.Domain.Events;

namespace Spt.Shared.Domain.Aggregates
{
    public interface IAggregateRootReference
    {
        AggregateRoot<TKey> GetAggregateRoot<TKey>();
        void RaiseEvent(DomainEvent @event);
    }
}
