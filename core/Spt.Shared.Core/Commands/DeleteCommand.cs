using Spt.Shared.Core.Models;
using System.Text.RegularExpressions;

namespace Spt.Shared.Core.Commands
{
    public class DeleteCommand<TBody> : Command<TBody, CommandResult>
    {
        public DeleteCommand()
        {
            Type = $"Delete{Regex.Replace(typeof(TBody).Name, "(Item|Dto)$", "")}";
        }

        public DeleteCommand(TBody body) : base(body)
        {
            Type = $"Delete{Regex.Replace(typeof(TBody).Name, "(Item|Dto)$", "")}";
        }
    }
}
