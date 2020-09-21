using MediatR;
using System;
using System.Runtime.Serialization;


namespace Spt.Shared.Core.Events
{
    [DataContract]
    public class Event : INotification
    {
        [DataMember]
        public virtual string Type { get; protected set; }

        [DataMember]
        public string Id { get; private set; }

        [DataMember]
        public string CorrelationId { get; private set; }

        [DataMember]
        public DateTimeOffset CreatedOn { get; private set; }

        public Event()
        {
            Type = GetType().Name;
            Id = Guid.NewGuid().ToString();
            CreatedOn = DateTimeOffset.Now;
        }

        public Event(string correlationId) : this()
        {
            CorrelationId = correlationId;
        }
    }
}
