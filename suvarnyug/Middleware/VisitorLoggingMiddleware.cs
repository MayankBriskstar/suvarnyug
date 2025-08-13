using Microsoft.AspNetCore.Http;
using Suvarnyug.Services;
using System.Threading.Tasks;

namespace Suvarnyug.Middlewares
{
    public class VisitorLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public VisitorLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, VisitorService visitorService)
        {
            // Log only if it's not a static file request
            if (!context.Request.Path.Value.Contains(".") &&
                !context.Request.Path.Value.StartsWith("/css") &&
                !context.Request.Path.Value.StartsWith("/js"))
            {
                await visitorService.LogVisitorAsync();
            }

            await _next(context);
        }
    }
}
