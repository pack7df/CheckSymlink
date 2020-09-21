using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Spt.Shared.AspNetCore.Middlewares
{
    public class AccessTokenFromQueryStringMiddleware
    {
        private readonly RequestDelegate _next;

        public AccessTokenFromQueryStringMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            if (context.Request.QueryString.HasValue)
            {
                if (string.IsNullOrWhiteSpace(context.Request.Headers["Authorization"]))
                {
                    var queryString = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(context.Request.QueryString.Value);
                    if (queryString.ContainsKey("access_token"))
                    {
                        string token = queryString["access_token"];

                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            context.Request.Headers.Add("Authorization", $"Bearer {token}");
                        }
                    }
                }
            }

            return _next(context);
        }
    }
}
