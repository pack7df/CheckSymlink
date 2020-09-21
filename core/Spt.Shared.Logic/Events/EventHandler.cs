using MediatR;
using Spt.Shared.Core.Events;
using Spt.Shared.Core.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Spt.Shared.Logic.Events
{
    public abstract class EventHandler<T> : INotificationHandler<T> where T : Event
    {
        protected Func<string, string> t { get; set; }

        public EventHandler()
        {
            var textTranslator = Core.ApplicationContext.ServiceProvider.GetService<ITextTranslator>();
            t = textTranslator == null ? (Func<string, string>)(o => o) : textTranslator.Translate;
        }

        public abstract Task Handle(T notification, CancellationToken cancellationToken);
    }
}
