using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ZiziBot.Presentation.Infrastructure.Middleware;

public class RequestBodyGuardMiddleware(ILogger<RequestBodyGuardMiddleware> logger) : IMiddleware
{
    readonly ApiResponseBase<object> _response = new();

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Check if body length is zero
        if (context.Request.Path.StartsWithSegments("/api") &&
            context.Request.ContentLength == 0)
        {
            logger.LogDebug("Request body is required");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            _response.TransactionId = context.GetTransactionId();

            await context.Response.WriteAsJsonAsync(_response.BadRequest("Request body is required"));
            return;
        }

        await next(context); // Proceed to the next middleware or endpoint
    }
}

