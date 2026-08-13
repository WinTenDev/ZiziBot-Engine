using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Flurl.Http;
using Flurl.Http.Configuration;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Enrichers.CallerInfo;
using Serilog.Events;
using Serilog.Sinks.AspNetCore.SignalR.Extensions;
using ZiziBot.Application.Features.LoggerSink;
using IHub = Serilog.Sinks.AspNetCore.SignalR.Interfaces.IHub;

namespace ZiziBot.Application.Infrastructure.Extensions;

public static class LoggingExtension
{
    // ReSharper disable InconsistentNaming
    private const string TEMPLATE_BASE = $"{{Message:lj}}{{NewLine}}{{Exception}}";
    private const string TEMPLATE_ENRICH = "{MemoryUsage} {ThreadId} {SourceContext} {Method} {SourceFile} {LineNumber}:{ColumnNumber}";

    private const string OUTPUT_TEMPLATE = $"{{Timestamp:HH:mm:ss.fff}} [{{Level:u3}}] {TEMPLATE_ENRICH} {TEMPLATE_BASE}";
    // ReSharper restore InconsistentNaming

    public static IHostBuilder ConfigureSerilogLite(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog((context, provider, config) =>
        {
            config.ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(provider)
                .MinimumLevel.Verbose()
                .WriteTo.Console(outputTemplate: OUTPUT_TEMPLATE)
                .Enrich.WithDemystifiedStackTraces();

            config.Enrich.WithDynamicProperty("MemoryUsage", () =>
            {
                var mem = Process.GetCurrentProcess().PrivateMemorySize64.Bytes().ToString("0.00");
                return $" {mem}";
            }).Enrich.WithDynamicProperty("ThreadId", () =>
            {
                var threadId = Environment.CurrentManagedThreadId.ToString();
                return $" {threadId}";
            });
        });

        return hostBuilder;
    }

    public static IHostBuilder ConfigureSerilog(this IHostBuilder hostBuilder, bool fullMode = false)
    {
        hostBuilder.UseSerilog((context, provider, config) =>
        {
            var appSettingRepository = provider.GetRequiredService<AppSettingRepository>();
            var sinkConfig = appSettingRepository.GetTelegramSinkConfig();

            var logConfig = appSettingRepository.GetRequiredConfigSection<EventLogConfig>();

            config.ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(provider)
                .MinimumLevel.Is(logConfig.LogLevel)
                .Enrich.WithDemystifiedStackTraces();

            if (logConfig.ProcessEnrich)
            {
                config.Enrich.WithDynamicProperty("MemoryUsage", () =>
                {
                    var mem = Process.GetCurrentProcess().PrivateMemorySize64.Bytes().ToString("0.00");
                    return $" MEM {mem} ";
                }).Enrich.WithDynamicProperty("ThreadId", () =>
                {
                    var threadId = Environment.CurrentManagedThreadId.ToString();
                    return $" Thread {threadId}";
                });
            }

            config.WriteTo.Async(cfg => cfg
                .Console(outputTemplate: OUTPUT_TEMPLATE)
                .WriteTo.SignalRSink<LogHub, IHub>(LogEventLevel.Debug, provider));

            if (!fullMode)
                return;

            var sentryConfig = appSettingRepository.GetConfigSection<SentryConfig>();

            if (sentryConfig?.IsEnabled ?? false)
            {
                config.WriteTo.Async(cfg =>
                {
                    cfg.Sentry(options =>
                    {
                        options.Dsn = sentryConfig.Dsn;
                        options.StackTraceMode = StackTraceMode.Enhanced;
                        options.Release = VersionUtil.GetVersion();
                    });
                });
            }

            config.WriteTo.Async(configuration =>
                configuration.Telegram(sinkConfig.BotToken, sinkConfig.ChatId, sinkConfig.ThreadId));
        });

        return hostBuilder;
    }

    public static IWebHostBuilder ConfigureSentry(this IWebHostBuilder hostBuilder)
    {
        if (Env.SentryDsn.IsNotNullOrWhiteSpace())
        {
            hostBuilder.UseSentry((context, options) =>
            {
                options.Dsn = Env.SentryDsn;
                options.TracesSampleRate = 1.0;
                options.ProfilesSampleRate = 1.0;
                options.Release = VersionUtil.GetVersion();
                options.AddProfilingIntegration();
            });

            SentrySdk.Init(options =>
            {
                options.Dsn = Env.SentryDsn;
                options.TracesSampleRate = 1.0;
                options.ProfilesSampleRate = 1.0;
                options.Release = VersionUtil.GetVersion();
                options.AddProfilingIntegration();
            });
        }

        return hostBuilder;
    }

    public static IServiceCollection AddSerilog(this IServiceCollection services, WebApplicationBuilder applicationBuilder)
    {
        services.AddSerilog((provider, config) =>
        {
            using var scope = provider.CreateScope();

            var appSettingRepository = scope.ServiceProvider.GetRequiredService<AppSettingRepository>();
            var sinkConfig = appSettingRepository.GetTelegramSinkConfig();

            var logConfig = appSettingRepository.GetRequiredConfigSection<EventLogConfig>();

            config.ReadFrom
                .Configuration(applicationBuilder.Configuration)
                .ReadFrom.Services(provider)
                .MinimumLevel.Is(logConfig.LogLevel)
                .MinimumLevel.Override("Hangfire", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
                .WriteTo.Console(outputTemplate: OUTPUT_TEMPLATE)
                .Enrich.FromLogContext()
                .Enrich.WithCallerInfo(includeFileInfo: true, assemblyPrefix: "ZiziBot", filePathDepth: 1)
                .Enrich.WithDemystifiedStackTraces();

            if (logConfig.ProcessEnrich)
            {
                config.Enrich.WithDynamicProperty("MemoryUsage", () =>
                {
                    var mem = Process.GetCurrentProcess().PrivateMemorySize64.Bytes().ToString("0.00");
                    return $"{mem}";
                }).Enrich.WithDynamicProperty("ThreadId", () =>
                {
                    var threadId = Environment.CurrentManagedThreadId.ToString("000000");
                    return $"{threadId}";
                });
            }

            if (logConfig.WriteToFile)
            {
                config.WriteTo.Async(cfg => cfg.File($"{PathConst.LOG}/log-.log",
                    outputTemplate: OUTPUT_TEMPLATE,
                    rollingInterval: RollingInterval.Day,
                    flushToDiskInterval: TimeSpan.FromSeconds(2),
                    shared: true
                ));
            }

            if (logConfig.WriteToSignalR)
            {
                config.WriteTo.Async(cfg => cfg.SignalRSink<LogHub, IHub>(logConfig.LogLevel, provider));
            }

            if (logConfig.WriteToTelegram)
            {
                config.WriteTo.Async(cfg => cfg.TelegramBatched(sinkConfig.BotToken, sinkConfig.ChatId, sinkConfig.ThreadId));
            }

            var sentryConfig = appSettingRepository.GetConfigSection<SentryConfig>();

            if (sentryConfig?.IsEnabled ?? false)
            {
                config.WriteTo.Async(cfg =>
                {
                    cfg.Sentry(options =>
                    {
                        options.Dsn = sentryConfig.Dsn;
                        options.StackTraceMode = StackTraceMode.Enhanced;
                        options.Release = VersionUtil.GetVersion();
                    });
                });
            }
        });

        return services;
    }

    public static IApplicationBuilder ConfigureFlurl(this IApplicationBuilder app)
    {
        var log = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(LoggingExtension));

        FlurlHttp.Clients.WithDefaults(builder =>
            {
                builder.Settings.JsonSerializer = new DefaultJsonSerializer(new JsonSerializerOptions() {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                });

                builder.BeforeCall(call =>
                {
                    var request = call.Request;
                    call.Request.Headers.Add("User-Agent", Env.COMMON_UA);

                    log.LogDebug("Flurl request {Method}: {Url}", request.Verb, request.Url);
                });

                builder.AfterCall(flurlCall =>
                {
                    var request = flurlCall.Request;
                    var response = flurlCall.Response;

                    log.LogInformation("Flurl response {Method}: {Url}: {StatusCode}. Elapsed: {Elapsed}",
                        request.Verb,
                        request.Url,
                        response?.StatusCode,
                        flurlCall.Duration
                    );
                });
            }
        );

        return app;
    }
}
