using Microsoft.AspNetCore.Http;

namespace ZiziBot.Presentation.Infrastructure.Middleware;

public class InjectRequestMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await next(context);
            return;
        }

        var ignorePaths = new[] {
            "/api/webhook",
            "/api/logging"
        };

        if (ignorePaths.Any(s => context.Request.Path.Value?.StartsWith(s) == true))
        {
            await next(context);
            return;
        }

        await next(context);
    }
}

