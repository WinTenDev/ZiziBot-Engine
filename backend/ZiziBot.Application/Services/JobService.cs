using Hangfire;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Features.UseCases.Rss;

namespace ZiziBot.Application.Services;

public class JobService(ILogger<JobService> logger)
{
    public async Task<bool> Register(long chatId, int threadId, string rssUrl, string jobId, bool triggerNow = false)
    {
        logger.LogDebug("Registering RSS Job. ChatId: {ChatId}, ThreadId: {ThreadId} RssUrl: {RssUrl}", chatId, threadId, rssUrl);

        RecurringJob.RemoveIfExists(jobId);
        RecurringJob.AddOrUpdate<FetchRssUseCase>(
            recurringJobId: jobId,
            methodCall: x => x.Handle(chatId,
                threadId,
                rssUrl
            ),
            cronExpression: TimeUtil.MinuteInterval(3)
        );

        if (triggerNow)
            RecurringJob.TriggerJob(jobId);

        await Task.Delay(1);

        return true;
    }
}