using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sentry.Hangfire;

namespace ZiziBot.Application.Features.Hangfire;

public static class HangfireServiceExtension
{
    internal static IServiceCollection ConfigureHangfire(this IServiceCollection services, IConfiguration configuration)
    {
        if (!EnvUtil.IsEnabled(Flag.HANGFIRE))
            return services;

        var hangfireConfig = configuration.GetSection("Hangfire").Get<HangfireConfig>() ?? new HangfireConfig();
        var jobStorage = CreateJobStorage(hangfireConfig);

        services.AddSingleton(jobStorage);

        services.AddHangfire((_, hangfireConfiguration) =>
        {
            hangfireConfiguration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseDarkDashboard()
                .UseStorage(jobStorage)
                .UseHeartbeatPage(checkInterval: TimeSpan.FromSeconds(3))
                .UseAppMediatorSerialization()
                .UseSentry()
                .UseSerilogLogProvider();
        });

        if (EnvUtil.IsEnabled(Flag.HANGFIRE_SEPARATED_SERVER))
        {
            foreach (var queue in hangfireConfig.Queues)
            {
                var queueName = queue.Name;
                var workerCount = queue.WorkerCount;

                services.AddHangfireServer(
                    optionsAction: (_, options) =>
                    {
                        options.WorkerCount = workerCount;
                        options.Queues = [queueName];
                    },
                    storage: jobStorage,
                    additionalProcesses: [
                        new ProcessMonitor(TimeSpan.FromSeconds(3))
                    ]
                );
            }
        }
        else
        {
            var queues = hangfireConfig.Queues.Select(x => x.Name).ToArray();

            services.AddHangfireServer(
                optionsAction: (_, options) =>
                {
                    options.WorkerCount = Environment.ProcessorCount * hangfireConfig.WorkerMultiplier;
                    options.Queues = queues;
                },
                storage: jobStorage,
                additionalProcesses: [
                    new ProcessMonitor(TimeSpan.FromSeconds(3))
                ]
            );
        }

        services.AddScoped<IDashboardAsyncAuthorizationFilter, HangfireAuthorizationFilter>();

        return services;
    }

    internal static IApplicationBuilder UseHangfire(this IApplicationBuilder app)
    {
        var serviceProvider = app.ApplicationServices;
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Hangfire");

        if (EnvUtil.IsEnabled(Flag.HANGFIRE))
        {
            using var scope = serviceProvider.CreateScope();
            var serviceScope = scope.ServiceProvider;
            var appSettingRepository = serviceScope.GetRequiredService<AppSettingRepository>();
            var config = appSettingRepository.GetConfigSection<HangfireConfig>();

            var dashboardOptions = new DashboardOptions() {
                DashboardTitle = config?.DashboardTitle ?? "Hangfire Dashboard",
                IgnoreAntiforgeryToken = false,
                AppPath = EnvUtil.GetEnv(Env.WEB_CONSOLE_URL)
            };

            if (EnvUtil.IsEnabled(Flag.HANGFIRE_ENABLE_AUTH))
            {
                var authorizationFilters = serviceScope.GetServices<IDashboardAuthorizationFilter>();
                var asyncAuthorizationFilters = serviceScope.GetServices<IDashboardAsyncAuthorizationFilter>();

                dashboardOptions.Authorization = authorizationFilters;
                dashboardOptions.AsyncAuthorization = asyncAuthorizationFilters;
            }

            app.UseHangfireDashboard(
                pathMatch: UrlConst.HANGFIRE_URL_PATH,
                options: dashboardOptions
            );
        }
        else
        {
            logger.LogInformation("Hangfire is disabled!");
        }

        return app;
    }

    private static JobStorage CreateJobStorage(HangfireConfig hangfireConfig)
    {
        return hangfireConfig.CurrentStorage switch {
            CurrentStorage.MongoDb => hangfireConfig.MongoDbConnection.ToMongoDbStorage(),
            CurrentStorage.Sqlite => StorageUtil.ToSqliteStorage(),
            CurrentStorage.LiteDb => StorageUtil.ToLiteDbStorage(),
            _ => new InMemoryStorage(new InMemoryStorageOptions())
        };
    }

    private static IGlobalConfiguration UseAppMediatorSerialization(this IGlobalConfiguration configuration)
    {
        var jsonSettings = new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.All
        };

        configuration.UseSerializerSettings(jsonSettings);

        return configuration;
    }
}
