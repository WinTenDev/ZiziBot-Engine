using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Features.Hangfire;

namespace ZiziBot.Application.Features.UseCases.Rss;

public class RegisterRssJobAllRequest
{
    public bool ResetStatus { get; set; }
}

public class RegisterRssJobAllUseCase(
    ILogger<RegisterRssJobAllUseCase> logger,
    RegisterRssJobUrlUseCase registerRssJobUrlUseCase,
    DataFacade dataFacade,
    ServiceFacade serviceFacade
)
{
    public async Task<bool> Handle(RegisterRssJobAllRequest request)
    {
        var transactionId = Guid.NewGuid().ToString();

        if (request.ResetStatus)
        {
            var rssSettingsAll = await dataFacade.MongoDb.RssSetting
                .Where(x => x.Status != EventStatus.Deleted)
                .ToListAsync();

            rssSettingsAll.ForEach(entity =>
            {
                entity.LastErrorMessage = string.Empty;
                entity.Status = EventStatus.Complete;
            });

            await dataFacade.MongoDb.SaveChangesAsync();
        }

        HangfireUtil.RemoveRssJobs();

        var rssSettings = await dataFacade.MongoDb.RssSetting
            .Where(entity => entity.Status == EventStatus.Complete)
            .ToListAsync();

        logger.LogInformation("Registering RSS Jobs. Count: {Count}", rssSettings.Count);

        await rssSettings.ParallelForEachAsync(async rss =>
        {
            var rssUrl = rss.OriginalRssUrl ?? string.Empty;

            try
            {
                var jobId = await StringUtil.GenerateRssKeyAsync();
                var newRssUrl = rssUrl.IsGithubReleaseUrl() ? rssUrl : await rssUrl.DetectRss(throwIfError: true);

                await registerRssJobUrlUseCase.Handle(new RegisterRssJobUrlRequest() {
                    ChatId = rss.ChatId,
                    ThreadId = rss.ThreadId,
                    Url = newRssUrl,
                    JobId = jobId
                });

                rss.RssUrl = newRssUrl;
                rss.CronJobId = jobId;
                rss.TransactionId = transactionId;
            }
            catch (Exception e)
            {
                rss.Status = EventStatus.Inactive;
                rss.LastErrorMessage = e.Message;

                logger.LogWarning("Error registering RSS Job. RssUrl: {RssUrl}, ChatId: {ChatId}, ThreadId: {ThreadId}. Message: {Message}",
                    rssUrl, rss.ChatId, rss.ThreadId, e.Message);
            }
        });

        await dataFacade.MongoDb.SaveChangesAsync();

        return true;
    }
}