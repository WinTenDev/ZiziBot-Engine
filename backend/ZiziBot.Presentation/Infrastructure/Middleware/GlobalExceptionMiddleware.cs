using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ZiziBot.Presentation.Infrastructure.Middleware;

public class GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled API Exception");

            var errorMessages = exception.ToStringDemystified().SplitWithTrimming(Environment.NewLine);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsJsonAsync(new ApiResponseBase<object>()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                TransactionId = context.GetTransactionId(),
                Message = !EnvUtil.IsProduction() ? exception.Message : "Internal Server Error. Please contact administrator.",
                ErrorMessages = !EnvUtil.IsProduction() ? errorMessages : null
            });
        }
    }
}

