using Microsoft.EntityFrameworkCore;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Interfaces;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Migrations;

public class RefreshAppSettingMigration(MongoDbContext mongoDbContext) : IPreMigration
{
    public async Task UpAsync()
    {
        var appSettings = await mongoDbContext.AppSettings.ToListAsync();
        foreach (var appSettingsEntity in appSettings)
        {
            if (string.IsNullOrEmpty(appSettingsEntity.Root))
                appSettingsEntity.Root = appSettingsEntity.Field.Split(":").FirstOrDefault();
        }

        await mongoDbContext.SaveChangesAsync();
    }

    public async Task DownAsync()
    { }
}
