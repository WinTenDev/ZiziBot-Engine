using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Infrastructure.Extensions;

public static class AppStartupExtension
{
    public static async Task<IApplicationBuilder> PrintAbout(this IApplicationBuilder app)
    {
        var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(AppStartupExtension));

        logger.LogInformation("ZiziBot 5");
        logger.LogInformation("Version: {Version}", VersionUtil.GetVersionNumber());

        return app;
    }

    public static async Task<IApplicationBuilder> PrefetchRepository(this IApplicationBuilder app)
    {
        await using var scope = app.ApplicationServices.CreateAsyncScope();
        var provider = scope.ServiceProvider;

        var featureFlagRepository = provider.GetRequiredService<FeatureFlagRepository>();
        await featureFlagRepository.GetFlags();

        var appSettingRepository = provider.GetRequiredService<AppSettingRepository>();
        var botRepository = provider.GetRequiredService<BotRepository>();
        var sentryConfig = await appSettingRepository.GetConfigSectionAsync<SentryConfig>();

        if (sentryConfig?.IsEnabled == true)
        {
            Env.SentryDsn = sentryConfig.Dsn;
        }

        await botRepository.GetApiKeyAsync(ApiKeyCategory.Internal, ApiKeyVendor.GitHub);
        await botRepository.GetBotMain();

        return app;
    }
}
