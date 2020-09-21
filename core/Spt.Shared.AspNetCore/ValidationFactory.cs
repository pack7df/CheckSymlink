using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Spt.Shared.Core.Models;
using System;
using System.Linq;

namespace Spt.Shared.AspNetCore
{
    public class ValidationFactory
    {
        public static Func<ActionContext, IActionResult> InvalidModelStateResponseFactory = context =>
        {
            var error = string.Join(".", context.ModelState.Values.SelectMany(o => o.Errors).Select(o => o.ErrorMessage.Trim(new char[] { ' ', '.' })));
            var result = new BadRequestObjectResult(new CommandResult<ModelStateDictionary>(false, "", error, context.ModelState));
            result.ContentTypes.Add("application/json");
            return result;
        };
    }
}
