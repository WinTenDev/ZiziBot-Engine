using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Features.Hangfire;

namespace ZiziBot.Application.Features.Handlers.Telegram.Rss;

public class RegisterRssJobAllRequest : IAppCommand<bool>, IRequest<bool>
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

            rssSettingsAll.ForEach(entity =>
            {
                entity.LastErrorMessage = string.Empty;
                entity.Status = EventStatus.Complete;
            });

            await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);
        }

        HangfireUtil.RemoveRssJobs();

        var rssSettings = await dataFacade.MongoDb.RssSetting
            .Where(entity => entity.Status == EventStatus.Complete)
            .ToListAsync(cancellationToken: cancellationToken);

        logger.LogInformation("Registering RSS Jobs. Count: {Count}", rssSettings.Count);

        foreach (var rssSettingEntity in rssSettings)
        {
            var jobId = await StringUtil.GenerateRssKeyAsync();

            await serviceFacade.Mediator.SendAsync(new RegisterRssJobUrlRequest {
                ChatId = rssSettingEntity.ChatId,
                ThreadId = rssSettingEntity.ThreadId ?? 0,
                Url = rssSettingEntity.RssUrl,
                JobId = jobId
            });

            rssSettingEntity.CronJobId = jobId;
            rssSettingEntity.TransactionId = Guid.NewGuid().ToString();
        }

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        return true;
    }
}
