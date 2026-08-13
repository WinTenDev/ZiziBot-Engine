using System.ComponentModel;
using Hangfire;
using Microsoft.Extensions.Logging;
using Sentry.Hangfire;
using ZiziBot.Application.Features.UseCases.Rss;

namespace ZiziBot.Application.Infrastructure.Scheduler;

public class RssScheduler(
    ILogger<RssScheduler> logger,
    RegisterRssJobAllUseCase registerRssJobAllUseCase,
    FetchRssUseCase fetchRssUseCase
)
{
    [DisplayName("RSS {0}:{1} {2}")]
    [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete, Attempts = 1)]
    [SentryMonitorSlug("FetchRssUseCase")]
    [Queue(CronJobKey.Queue_Rss)]
    public async Task RssBroadcast(long chatId, int threadId, string rssUrl)
    {
        await fetchRssUseCase.Handle(chatId: chatId, rssUrl: rssUrl, threadId: threadId);
    }

    [DisplayName("RSS Refresh")]
    [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete, Attempts = 3)]
    [SentryMonitorSlug("RegisterRssJobAllUseCase")]
    [Queue(CronJobKey.Queue_Data)]
    public async Task RssRefresh()
    {
        await registerRssJobAllUseCase.Handle(new RegisterRssJobAllRequest() {
            ResetStatus = true
        });
    }
}