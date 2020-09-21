using MediatR;
using Spt.Shared.Core.Commands;
using Spt.Shared.Core.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Spt.Shared.Logic.Commands
{
    public abstract class CommandHandler<TCommand> : CommandHandler<TCommand, MediatR.Unit> where TCommand : Command<MediatR.Unit>
    {
        protected CommandHandler() : base()
        {
        }
    }

    public abstract class CommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse> where TCommand : Command<TResponse>
    {
        protected IEventRouter EventRouter { get; }
        protected IQueryRouter QueryRouter { get; }
        protected Func<string, string> t { get; set; }

        public CommandHandler()
        {
            var textTranslator = Core.ApplicationContext.ServiceProvider.GetService<ITextTranslator>();
            t = textTranslator == null ? (Func<string, string>)(o => o) : textTranslator.Translate;
            EventRouter = Core.ApplicationContext.ServiceProvider.GetService<IEventRouter>();
            QueryRouter = Core.ApplicationContext.ServiceProvider.GetService<IQueryRouter>();
        }

        public abstract Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken);
    }
}
