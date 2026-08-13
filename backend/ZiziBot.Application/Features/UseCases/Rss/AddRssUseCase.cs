using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Features.UseCases.Rss;

public struct AddRssParam
{
    public long ChatId { get; set; }
    public long UserId { get; set; }
    public int ThreadId { get; set; }
    public string Url { get; set; }
}

public class AddRssUseCase(
    ILogger<AddRssUseCase> logger,
    DataFacade dataFacade,
    ServiceFacade serviceFacade
)
{
    public async Task<string> Handle(AddRssParam request)
    {
        logger.LogInformation("Detecting RSS: {Rss}", request.Url);

        var rssUrl = await request.Url.DetectRss();

        var rssSetting = await dataFacade.MongoDb.RssSetting
            .Where(entity => entity.RssUrl == rssUrl)
            .Where(entity => entity.ChatId == request.ChatId)
            .Where(entity => entity.ThreadId == request.ThreadId)
            .Where(entity => entity.Status == EventStatus.Complete)
            .FirstOrDefaultAsync();

        var uniqueId = await StringUtil.GenerateRssKeyAsync();

        if (rssSetting == null)
        {
            logger.LogDebug("Adding RSS: {Rss}", rssUrl);
            var create = dataFacade.MongoDb.RssSetting.Add(new RssSettingEntity {
                ChatId = request.ChatId,
                RssUrl = rssUrl,
                OriginalRssUrl = rssUrl,
                ThreadId = request.ThreadId,
                UserId = request.UserId,
                CronJobId = uniqueId,
                Status = EventStatus.Complete
            });
        }

        if (rssSetting != null)
        {
            rssSetting.CronJobId = uniqueId;
            rssSetting.OriginalRssUrl = rssUrl;
        }

        await serviceFacade.JobService.Register(request.ChatId, request.ThreadId, rssUrl, uniqueId, true);
        await dataFacade.MongoDb.SaveChangesAsync();

        logger.LogInformation("Final RSS: {Rss}", rssUrl);

        return rssUrl;
    }
}
