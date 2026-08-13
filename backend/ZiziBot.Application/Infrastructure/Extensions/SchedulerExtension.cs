using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZiziBot.Application.Features.Hangfire;

namespace ZiziBot.Application.Infrastructure.Extensions;

public static class SchedulerExtension
{
    public static IServiceCollection ConfigureScheduler(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<SchedulerService>();
        services.ConfigureHangfire(configuration);

        return services;
    }

    public static IApplicationBuilder UseScheduler(this IApplicationBuilder app)
    {
        app.UseHangfire();

        return app;
    }
}
