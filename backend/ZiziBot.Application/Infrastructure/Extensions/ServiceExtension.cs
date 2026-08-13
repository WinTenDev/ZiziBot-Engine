using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using DalSoft.Hosting.BackgroundQueue.DependencyInjection;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using ZiziBot.Application.Infrastructure.Database.Extensions;

namespace ZiziBot.Application.Infrastructure.Extensions;

public static class ServiceExtension
{
    public static IServiceProvider GlobalServiceProvider { get; set; }

    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddAllService();
        services.ConfigureSettings();
        services.ConfigureFusionCache();
        services.AddCacheTower();
        services.AddMongoMigration();
        services.AddApplicationCortexMediator();
        services.ConfigureBackgroundQueue();
        services.ConfigureFlurl();

        return services;
    }

    private static IServiceCollection ConfigureFlurl(this IServiceCollection services)
    {
        services.AddSingleton<IFlurlClientCache>(sp => new FlurlClientCache()
            .WithDefaults(builder =>
                {
                    builder.Settings.JsonSerializer = new DefaultJsonSerializer(new JsonSerializerOptions()
                    {
                        NumberHandling = JsonNumberHandling.AllowReadingFromString
                    });

                    builder.BeforeCall(call =>
                    {
                        var request = call.Request;
                        call.Request.Headers.Add("User-Agent", Env.COMMON_UA);

                        Log.Information("FlurlHttp: {Method} {Url}", request.Verb, request.Url);
                    });

                    builder.AfterCall(flurlCall =>
                    {
                        var request = flurlCall.Request;
                        var response = flurlCall.Response;

                        Log.Information("FlurlHttp: {Method} {Url}: {StatusCode}. Elapsed: {Elapsed}",
                            request.Verb,
                            request.Url,
                            response?.StatusCode,
                            flurlCall.Duration
                        );
                    });
                }
            ));

        return services;
    }

    private static IServiceCollection AddAllService(this IServiceCollection services)
    {
        var assembly = Assembly.Load("ZiziBot.Application");

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        services.Scan(selector => selector.FromAssembliesOf(typeof(TaskRunnerHostedService))
            .AddClasses(filter => filter.InNamespaceOf<TaskRunnerHostedService>())
            .As<IHostedService>()
            .WithSingletonLifetime());

        services.Scan(selector => selector.FromAssemblies(assembly)
            .AddClasses(x => {
                x.InNamespaces(
                    "ZiziBot.Application.Services",
                    "ZiziBot.Application.Features.UseCases"
                );
                x.Where(type => !typeof(IHostedService).IsAssignableFrom(type));
            })
            .AsSelf()
            .WithScopedLifetime());

        services.Scan(selector => selector.FromAssembliesOf(typeof(RegisterRssJobTask))
            .AddClasses(filter => filter.InNamespaceOf<RegisterRssJobTask>())
            .As<IStartupTask>()
            .WithScopedLifetime());

        services.Scan(selector => selector.FromAssembliesOf(typeof(MirrorPaymentRestService))
            .AddClasses(filter => {
                filter.InNamespaceOf<MirrorPaymentRestService>();
                filter.Where(type => !typeof(IHostedService).IsAssignableFrom(type));
            })
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        services.AddScoped<ServiceFacade>();
        services.AddSingleton<IServiceScopedUtil, ServiceScopedUtil>();

        return services;
    }

    private static IServiceCollection ConfigureBackgroundQueue(this IServiceCollection services)
    {
        services.AddBackgroundQueue(exception => Log.Error("Error when run background job {0}", exception.Message));

        return services;
    }
}
