using dotenv.net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZiziBot.Application.Infrastructure.Config.MongoConfig;
using ZiziBot.Application.Infrastructure.Database.Extensions;
using ZiziBot.Application.Common.Exceptions;

namespace ZiziBot.Application.Infrastructure.Extensions;

public static class ConfigurationExtension
{
    public static IConfigurationBuilder LoadSettings(this IConfigurationBuilder builder)
    {
        DotEnv.Load();

        builder.AddMongoConfigurationSource();

        return builder.LoadLocalSettings();
    }

    public static IServiceCollection ConfigureSettings(this IServiceCollection services)
    {
        services.AddDataSource();
        services.AddDataRepository();

        services.AddOptions<CacheConfig>().BindConfiguration("Cache");
        services.AddOptions<EngineConfig>().BindConfiguration("Engine");
        services.AddOptions<EventLogConfig>().BindConfiguration("EventLog");
        services.AddOptions<GcpConfig>().BindConfiguration("Gcp");
        services.AddOptions<HangfireConfig>().BindConfiguration("Hangfire");
        services.AddOptions<JwtConfig>().BindConfiguration("Jwt");
        services.AddOptions<OptiicDevConfig>().BindConfiguration("OptiicDev");

        return services;
    }

    private static IConfigurationBuilder LoadLocalSettings(this IConfigurationBuilder builder)
    {
        var settingsPath = Path.Combine(Environment.CurrentDirectory, "Storage", "AppSettings", "Current");

        if (!Directory.Exists(settingsPath))
        {
            return builder;
        }

        var settingFiles = Directory.GetFiles(settingsPath)
            .Where(file => !file.EndsWith("x.json")) // End with x.json to ignore
            .ToList();

        settingFiles.ForEach(file => builder.AddJsonFile(file, reloadOnChange: true, optional: false));

        return builder;
    }

    private static IConfigurationBuilder AddMongoConfigurationSource(this IConfigurationBuilder builder)
    {
        var mongodbConnectionString = EnvUtil.GetEnv(Env.MONGODB_CONNECTION_STRING, throwIsMissing: true);
        var url = mongodbConnectionString.ToMongoUrl();

        if (url.DatabaseName.IsNullOrEmpty())
        {
            throw new AppException("Database name is not specified in Connection String. Example: mongodb://localhost:27017/DatabaseName");
        }

        builder.Add(new MongoConfigSource(mongodbConnectionString));

        return builder;
    }
}
