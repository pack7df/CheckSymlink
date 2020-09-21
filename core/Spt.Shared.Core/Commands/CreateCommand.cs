using Spt.Shared.Core.Models;
using System.Text.RegularExpressions;

namespace Spt.Shared.Core.Commands
{
    public class CreateCommand<TBody> : Command<TBody, CommandResult>
    {
        public CreateCommand()
        {
            Type = $"Create{Regex.Replace(typeof(TBody).Name, "(Item|Dto)$", "")}";
        }

        public CreateCommand(TBody body) : base(body)
        {
            Type = $"Create{Regex.Replace(typeof(TBody).Name, "(Item|Dto)$", "")}";
        }
    }
}
