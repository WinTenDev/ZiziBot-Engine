using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;
using DnsClient.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace ZiziBot.Presentation.Extensions;

public static class RateLimiterExtension
{
    public static IServiceCollection ConfigureRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(limiterOptions => {
            limiterOptions.AddSlidingWindowLimiter(RateLimitingPolicy.API_LIST_RATE_LIMITING_KEY, options => {
                options.PermitLimit = 4;
                options.Window = TimeSpan.FromSeconds(2);
                options.SegmentsPerWindow = 2;
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 2;
            });

            limiterOptions.OnRejected += OnRejected;
        });

        return services;
    }

    public static IApplicationBuilder ConfigureRateLimiter(this IApplicationBuilder app)
    {
        app.UseRateLimiter();

        return app;
    }

    private static ValueTask OnRejected(OnRejectedContext context, CancellationToken cancellationToken)
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
        }

        context.HttpContext.RequestServices.GetService<ILoggerFactory>()?
            .CreateLogger("Microsoft.AspNetCore.RateLimitingMiddleware")
            .LogWarning("OnRejected: {GetUserEndPoint}", context.HttpContext.Request.Path);

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.WriteAsJsonAsync(new ApiResponseBase<object>() {
            StatusCode = HttpStatusCode.TooManyRequests,
            TransactionId = context.HttpContext.Request.Headers[RequestKey.TransactionId].ToString(),
            Message = "Too Many Requests"
        }, cancellationToken: cancellationToken);

        return new ValueTask();
    }
}

