using Spt.Shared.Core.Models;
using System.Text.RegularExpressions;

namespace Spt.Shared.Core.Commands
{
    public class UpdateCommand<TBody> : Command<TBody, CommandResult>
    {
        public UpdateCommand()
        {
            Type = $"Update{Regex.Replace(typeof(TBody).Name, "(Item|Dto)$", "")}";
        }

        public UpdateCommand(TBody body) : base(body)
        {
            Type = $"Update{Regex.Replace(typeof(TBody).Name, "(Item|Dto)$", "")}";
        }
    }
}
