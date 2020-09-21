using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Spt.Shared.AspNetCore.Middlewares
{
    public class ApplicationInsightsLoggingMiddleware : IMiddleware
    {
        private TelemetryClient TelemetryClient { get; }

        public ApplicationInsightsLoggingMiddleware(TelemetryClient telemetryClient)
        {
            TelemetryClient = telemetryClient;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Inbound (before the controller)
            var request = context?.Request;
            if (request == null)
            {
                await next(context);
                return;
            }

            request.EnableBuffering();  // Allows us to reuse the existing Request.Body

            // Swap the original Response.Body stream with one we can read / seek
            var originalResponseBody = context.Response.Body;
            using (var replacementResponseBody = new MemoryStream())
            {
                context.Response.Body = replacementResponseBody;

                await next(context); // Continue processing (additional middleware, controller, etc.)

                // Outbound (after the controller)
                replacementResponseBody.Position = 0;

                // Copy the response body to the original stream
                await replacementResponseBody.CopyToAsync(originalResponseBody).ConfigureAwait(false);
                context.Response.Body = originalResponseBody;

                var response = context.Response;
                if (response.StatusCode < 500)
                {
                    return;
                }

                var requestTelemetry = context.Features.Get<RequestTelemetry>();
                if (requestTelemetry == null)
                {
                    return;
                }

                if (request.Body.CanRead)
                {
                    var requestBodyString = await ReadBodyStream(request.Body).ConfigureAwait(false);
                    requestTelemetry.Properties.Add("RequestBody", requestBodyString);  // limit: 8192 characters
                    TelemetryClient.TrackTrace(requestBodyString);
                }

                if (replacementResponseBody.CanRead)
                {
                    var responseBodyString = await ReadBodyStream(replacementResponseBody).ConfigureAwait(false);
                    requestTelemetry.Properties.Add("ResponseBody", responseBodyString);
                    TelemetryClient.TrackTrace(responseBodyString);
                }
            }
        }

        private async Task<string> ReadBodyStream(Stream body)
        {
            if (body.Length == 0)
            {
                return null;
            }

            body.Position = 0;

            using (var reader = new StreamReader(body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true))
            {
                var bodyString = await reader.ReadToEndAsync().ConfigureAwait(false);
                body.Position = 0;

                return bodyString;
            }
        }
    }
}
