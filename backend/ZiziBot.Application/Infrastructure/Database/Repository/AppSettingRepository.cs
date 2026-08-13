using Microsoft.EntityFrameworkCore;
using ZiziBot.Application.Infrastructure.Database.MongoDb;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Infrastructure.Database.Repository;

public class AppSettingRepository(
    MongoDbContext mongoDbContext
)
{
    public TelegramSinkConfigDto GetTelegramSinkConfig()
    {
        var botToken = mongoDbContext.BotSettings.Select(x => new { x.Name, x.Token }).FirstOrDefault(entity => entity.Name == "Main");

        var eventLogConfig = GetConfigSection<EventLogConfig>();

        return new TelegramSinkConfigDto()
        {
            BotToken = botToken?.Token,
            ChatId = eventLogConfig?.ChatId,
            ThreadId = eventLogConfig?.ThreadId
        };
    }

    public T GetRequiredConfigSection<T>() where T : new()
    {
        var config = GetConfigSection<T>();

        return config ?? throw new InvalidOperationException("Config section not found");
    }

    public async Task<T> GetRequiredConfigSectionAsync<T>() where T : new()
    {
        var config = await GetConfigSectionAsync<T>();

        return config ?? throw new InvalidOperationException("Config section not found");
    }

    public async Task<T?> GetConfigSectionAsync<T>() where T : new()
    {
        var rootConfig = typeof(T).Name.Replace("Config", "");

        var appSettings = await GetConfigSectionInternalAsync(rootConfig);

        var data = appSettings
            .DistinctBy(d => d.Name)
            .ToDictionary(x => x.Name.Remove(0, rootConfig.Length + 1), x => x.Value).ToJson().ToObject<T>();

        return data;
    }

    public T? GetConfigSection<T>() where T : new()
    {
        var rootConfig = typeof(T).Name.Replace("Config", "");

        var appSettings = GetConfigSectionInternal(rootConfig);

        var data = appSettings
            .DistinctBy(d => d.Name)
            .ToDictionary(x => x.Name.Remove(0, rootConfig.Length + 1), x => x.Value).ToJson().ToObject<T>();

        return data;
    }

    private List<AppSettingsEntity> GetConfigSectionInternal(string rootConfig)
    {
        var appSettings = mongoDbContext.AppSettings.AsNoTracking()
            .Where(entity => entity.Root == rootConfig)
            .ToList();

        return appSettings;
    }

    private async Task<List<AppSettingsEntity>> GetConfigSectionInternalAsync(string rootConfig)
    {
        var appSettings = await mongoDbContext.AppSettings.AsNoTracking()
            .Where(entity => entity.Root == rootConfig)
            .ToListAsync();

        return appSettings;
    }

    public async Task UpdateAppSetting(string name, string value)
    {
        var appSettings = await mongoDbContext.AppSettings
            .Where(entity => entity.Name == name)
            .Where(entity => entity.Status == EventStatus.Complete)
            .FirstOrDefaultAsync();

        if (appSettings == null)
            return;

        appSettings.Value = value;

        await mongoDbContext.SaveChangesAsync();
    }
}
