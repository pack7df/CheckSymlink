using Spt.Shared.Core.Events;
using System.Runtime.Serialization;

namespace Spt.Shared.Domain.Events
{
    [DataContract]
    public class DomainEvent : Event
    {
        [DataMember]
        public string AggregateId { get; private set; }

        public DomainEvent() : base()
        {
        }

        public DomainEvent(string aggregateId, string correlationId) : base(correlationId)
        {
            AggregateId = aggregateId;
        }
    }
}
