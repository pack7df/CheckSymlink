using MediatR;
using System.Runtime.Serialization;

namespace Spt.Shared.Core.Commands
{
    [DataContract]
    public class Command<TRequest, TResponse> : Command<TResponse>
    {
        [DataMember]
        public TRequest Body { get; }

        public Command() : base()
        {
        }

        public Command(TRequest body) : this()
        {
            Body = body;
        }
    }

    [DataContract]
    public class Command<TResponse> : IRequest<TResponse>
    {
        [DataMember]
        public virtual string Type { get; protected set; }

        public Command()
        {
            Type = GetType().Name;
        }
    }

    [DataContract]
    public class Command : Command<MediatR.Unit>
    {
        public Command() : base()
        {
        }
    }
}
