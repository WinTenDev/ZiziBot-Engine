using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ZiziBot.WebApi.Middlewares;

public class HeaderCheckMiddleware : IMiddleware
{
    private readonly ILogger<HeaderCheckMiddleware> _logger;

    public HeaderCheckMiddleware(ILogger<HeaderCheckMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        _logger.LogTrace("Request Context: {Context}", context.Request.Path);

        if (!(context.Request.Path.Value?.StartsWith("/api") ?? false))
        {
            _logger.LogTrace("Url is not api");
            await next(context);
            return;
        }

        var ignorePaths = new[]
        {
            "/api/webhook",
            "/api/logging"
        };

        if (ignorePaths.Any(s => context.Request.Path.Value.StartsWith(s)))
        {
            _logger.LogDebug("Url path is ignored from Header verification. Url: {Url}", context.Request.Path.Value);
            await next(context);
            return;
        }

        if (!context.Request.Headers.ContainsKey(HeaderKey.TransactionId))
        {
            await ReturnBadRequest(context, "Please set 'transactionId' in Headers");
            return;
        }

        await next(context);
    }

    private static async Task ReturnBadRequest(HttpContext context, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        await context.Response.WriteAsJsonAsync(new ApiResponseBase<bool>()
        {
            StatusCode = HttpStatusCode.BadRequest,
            Message = message
        });
    }
}