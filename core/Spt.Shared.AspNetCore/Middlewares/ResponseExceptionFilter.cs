using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.ComponentModel.DataAnnotations;

namespace Spt.Shared.AspNetCore.Middlewares
{
    public class ResponseExceptionFilter : IActionFilter, IOrderedFilter
    {
        public int Order { get; set; } = int.MaxValue - 10;

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            //var telemetry = context.HttpContext.RequestServices.GetService<TelemetryClient>();
            if (context.Exception != null)
            {
                //telemetry?.TrackException(context.Exception);
                context.Result = new ObjectResult(new
                {
                    isSuccess = false,
                    error = GetMessage(context.Exception),
#if DEBUG
                    stackTrace = context.Exception.StackTrace
#endif
                })
                {
                    StatusCode = SelectStatusCode(context.Exception),
                    ContentTypes = new Microsoft.AspNetCore.Mvc.Formatters.MediaTypeCollection() { new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("application/json") }
                };
                context.ExceptionHandled = true;
            }
        }

        private string GetMessage(Exception exception)
        {
            var sb = new System.Text.StringBuilder();
            var ex = exception;
            var addSpace = false;
            while (ex != null)
            {
                if (addSpace)
                    sb.Append(" ");
                sb.Append(ex.Message);
                addSpace = true;
                ex = ex.InnerException;
            }
            return sb.ToString();
        }

        private int SelectStatusCode(Exception exception)
        {
            if ((exception is ArgumentException) ||
                (exception.GetType().Name == "DbUpdateException") ||
                (exception is ValidationException))
                return 400;
            return 500;
        }
    }
}
