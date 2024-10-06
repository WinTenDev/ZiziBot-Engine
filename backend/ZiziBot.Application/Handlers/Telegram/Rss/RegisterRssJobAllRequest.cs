using Microsoft.Extensions.Logging;
using MongoFramework.Linq;

namespace ZiziBot.Application.Handlers.Telegram.Rss;

public class RegisterRssJobAllRequest : IRequest<bool>
{
    public bool ResetStatus { get; set; }
}

public class RegisterRssJobAllHandler(
    ILogger<RegisterRssJobAllHandler> logger,
    DataFacade dataFacade,
    ServiceFacade serviceFacade
) : IRequestHandler<RegisterRssJobAllRequest, bool>
{
    public async Task<bool> Handle(RegisterRssJobAllRequest request, CancellationToken cancellationToken)
    {
        if (request.ResetStatus)
        {
            var rssSettingsAll = await dataFacade.MongoDb.RssSetting.ToListAsync(cancellationToken: cancellationToken);
            rssSettingsAll.ForEach(entity => {
                entity.LastErrorMessage = string.Empty;
                entity.Status = (int)EventStatus.Complete;
            });

            await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);
        }

        HangfireUtil.RemoveRssJobs();

        var rssSettings = await dataFacade.MongoDb.RssSetting
            .Where(entity => entity.Status == (int)EventStatus.Complete)
            .ToListAsync(cancellationToken: cancellationToken);

        logger.LogInformation("Registering RSS Jobs. Count: {Count}", rssSettings.Count);

        foreach (var rssSettingEntity in rssSettings)
        {
            var jobId = await StringUtil.GetNanoIdAsync(prefix: "RssJob:", size: 7);

            await serviceFacade.Mediator.Send(new RegisterRssJobUrlRequest {
                ChatId = rssSettingEntity.ChatId,
                ThreadId = rssSettingEntity.ThreadId,
                Url = rssSettingEntity.RssUrl,
                JobId = jobId
            });

            rssSettingEntity.CronJobId = jobId;
        }

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        return true;
    }
}