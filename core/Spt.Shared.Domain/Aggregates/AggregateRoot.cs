using Spt.Shared.Domain.Entities;
using Spt.Shared.Domain.Events;
using System.Runtime.Serialization;

namespace Spt.Shared.Domain.Aggregates
{
    [DataContract]
    public class AggregateRoot<TKey> : Entity<TKey>, IAggregateRootReference
    {
        public AggregateRoot(TKey id) : base(id)
        {

        }

        public AggregateRoot() : base()
        {

        }

        public AggregateRoot<TRequestedKey> GetAggregateRoot<TRequestedKey>()
        {
            return (AggregateRoot<TRequestedKey>)(object)this;
        }

        void IAggregateRootReference.RaiseEvent(DomainEvent @event)
        {
            RaiseEvent(@event);
        }
    }
}
