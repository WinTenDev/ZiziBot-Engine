using Microsoft.EntityFrameworkCore;
using ZiziBot.Application.Infrastructure.Database.MongoDb;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Infrastructure.Database.Repository;

public class RssRepository(MongoDbContext mongoDbContext)
{
    public async Task<RssHistoryEntity?> GetLastRssArticle(long chatId, int? threadId, string articleUrl)
    {
        var lastArticle = await mongoDbContext.RssHistory.AsNoTracking()
            .Where(entity => entity.ChatId == chatId)
            .WhereIf(threadId > 0, entity => entity.ThreadId == threadId)
            .Where(entity => entity.Url == articleUrl)
            .Where(entity => entity.Status == EventStatus.Complete)
            .FirstOrDefaultAsync();

        return lastArticle;
    }
}
