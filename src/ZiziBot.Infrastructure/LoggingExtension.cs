using Flurl.Http;
using HelpMate.Core.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ZiziBot.Infrastructure;

public static class LoggingExtension
{
    private const string TEMPLATE_BASE = $"[{{Level:u3}}]{{MemoryUsage}}{{ThreadId}} {{Message:lj}}{{NewLine}}{{Exception}}";
    private const string OUTPUT_TEMPLATE = $"{{Timestamp:HH:mm:ss.fff}} {TEMPLATE_BASE}";

    public static IHostBuilder InitSerilogBootstrapper(this IHostBuilder hostBuilder, bool fullMode = false)
    {
        hostBuilder.UseSerilog((context, provider, config) => {
            config.ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(provider)
                .MinimumLevel.Debug()
                .Enrich.WithDemystifiedStackTraces();

            config.WriteTo.Async(configuration => configuration.Console(outputTemplate: OUTPUT_TEMPLATE));

            if (!fullMode)
                return;

            var appDbContext = provider.GetRequiredService<AppSettingsDbContext>();

            var chatId = appDbContext.AppSettings.FirstOrDefault(entity => entity.Name == "EventLog:ChatId")?.Value;
            var botToken = appDbContext.BotSettings.FirstOrDefault(entity => entity.Name == "Main")?.Token;

            config.WriteTo.Async(configuration => configuration.Telegram(botToken, chatId.ToInt64()));
        });

        return hostBuilder;
    }

    public static IApplicationBuilder ConfigureFlurlLogging(this IApplicationBuilder app)
    {
        FlurlHttp.Configure(
            settings => {
                settings.BeforeCall = flurlCall => {
                    var request = flurlCall.Request;
                    Log.Information("FlurlHttp: {Method} {url}", request.Verb, request.Url);
                };

                settings.AfterCall = flurlCall => {
                    var request = flurlCall.Request;
                    var response = flurlCall.Response;
                    Log.Information(
                        "FlurlHttp: {Method} {Url} {StatusCode}. Elapsed: {Elapsed}",
                        request.Verb,
                        request.Url,
                        response?.StatusCode,
                        flurlCall.Duration
                    );
                };
            }
        );

        return app;
    }
}