using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ZiziBot.Application.Infrastructure.Database.MongoDb;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Infrastructure.Config.MongoConfig;

public class MongoConfigSource(string connectionString) : IConfigurationSource
{
    private readonly MongoDbContext _dbContext = new();

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new MongoConfigProvider(this);
    }

    public Dictionary<string, string?> GetAppSettings()
    {
        SeedAppSettings();

        var appSettingsList = _dbContext.AppSettings.AsNoTracking()
            .Where(x => x.Status == EventStatus.Complete)
            .Select(x => new KeyValuePair<string, string?>(x.Name, x.Value))
            .ToList();

        return appSettingsList
            .DistinctBy(pair => pair.Key)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    private void SeedAppSettings()
    {
        var defaultAppSettings = ConfigUtil.GetAppDefault();
        var appSettings = _dbContext.AppSettings.AsNoTracking()
            .Where(x => x.Status == EventStatus.Complete)
            .ToList();

        var groupedAppSettings = appSettings.GroupBy(x => x.Name).Select(x => x.Last()).ToList();

        var appSettingsDict = groupedAppSettings.ToDictionary(x => x.Name, x => x.Value.ToString());
        var seedableAppSetting = defaultAppSettings
            .SelectMany(s => s.KeyPair, (s, kv) => new
            {
                Root = s.Root,
                Key = $"{s.Root}:{kv.Key}",
                Value = kv.Value.ToString(),
                ValueType = kv.Value.GetType()
            })
            .Where(kv => !appSettingsDict.ContainsKey(kv.Key))
            .ToList();

        var transactionId = Guid.NewGuid().ToString();

        seedableAppSetting.ForEach(seed =>
        {
            var appSetting = _dbContext.AppSettings.AsNoTracking()
                .Where(x => x.Status == EventStatus.Complete)
                .FirstOrDefault(settings => settings.Name == seed.Key);

            if (appSetting != null) return;

            _dbContext.AppSettings.Add(new AppSettingsEntity
            {
                Root = seed.Root,
                Name = seed.Key,
                Field = seed.Key,
                DefaultValue = $"{seed.Value}",
                InitialValue = $"{seed.Value}",
                Value = $"{seed.Value}",
                TransactionId = transactionId,
                Status = EventStatus.Complete
            });
        });

        _dbContext.SaveChanges();
    }
}
