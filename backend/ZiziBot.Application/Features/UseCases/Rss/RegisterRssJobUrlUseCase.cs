using Hangfire;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Infrastructure.Scheduler;

namespace ZiziBot.Application.Features.UseCases.Rss;

public class RegisterRssJobUrlRequest
{
    public long ChatId { get; set; }
    public int? ThreadId { get; set; }
    public string Url { get; set; }
    public string JobId { get; set; }
}

public class RegisterRssJobUrlUseCase(
    ILogger<RegisterRssJobUrlUseCase> logger,
    ServiceFacade serviceFacade
)
{
    public async Task<bool> Handle(RegisterRssJobUrlRequest request)
    {
        logger.LogDebug("Registering RSS Job. ChatId: {ChatId}, ThreadId: {ThreadId}, RssUrl: {RssUrl}", request.ChatId, request.ThreadId, request.Url);

        RecurringJob.RemoveIfExists(request.JobId);
        RecurringJob.AddOrUpdate<RssScheduler>(
            request.JobId,
            methodCall: x => x.RssBroadcast(request.ChatId, request.ThreadId ?? 0, request.Url),
            queue: CronJobKey.Queue_Rss,
            cronExpression: TimeUtil.MinuteInterval(3)
        );

        await Task.Delay(1);

        return true;
    }
}