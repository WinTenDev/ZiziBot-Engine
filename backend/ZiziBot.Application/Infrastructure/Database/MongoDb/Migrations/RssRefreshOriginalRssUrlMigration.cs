using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Interfaces;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Migrations;

public class RssRefreshOriginalRssUrlMigration(ILogger<RssRefreshOriginalRssUrlMigration> logger, MongoDbContext mongoDbContext) : IPostMigration
{
    public async Task UpAsync()
    {
        var listRssSetting = await mongoDbContext.RssSetting
            .Where(x => string.IsNullOrWhiteSpace(x.OriginalRssUrl))
            .ToListAsync();

        listRssSetting.ForEach(entity =>
        {
            logger.LogDebug("Update RSS Original url to {RssUrl}", entity.RssUrl);
            entity.OriginalRssUrl = entity.RssUrl;
        });

        await mongoDbContext.SaveChangesAsync();
    }

    public Task DownAsync()
    {
        throw new NotImplementedException();
    }
}
