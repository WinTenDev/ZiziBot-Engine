using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;

namespace ZiziBot.Application.Features.TickerQ;

public static class TickerQServiceExtension
{
    public static IServiceCollection ConfigureTickerQ(this IServiceCollection services)
    {
        services.AddTickerQ(o =>
        {
            // o.SetMaxConcurrency(4);
            o.AddDashboard(configuration =>
            {
                configuration.SetBasePath("/admin/ticker-q");
            });

            // o.AddOperationalStore<MongoDbContext>(builder =>
            // {
            //     builder.CancelMissedTickersOnApplicationRestart();
            // });
        });

        return services;
    }

    public static IApplicationBuilder EnableTickerQ(this IApplicationBuilder app)
    {
        // app.UseTickerQ();

        return app;
    }
}