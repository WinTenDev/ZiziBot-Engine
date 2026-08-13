using Hangfire;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Infrastructure.Scheduler;

namespace ZiziBot.Application.Features.Handlers.Telegram.Rss;

public class RegisterRssJobUrlRequest : IAppCommand<bool>, IRequest<bool>
{
    public long ChatId { get; set; }
    public int ThreadId { get; set; }
    public string Url { get; set; }
    public string JobId { get; set; }
}

public class RegisterRssJobUrlHandler(
    ILogger<RegisterRssJobUrlHandler> logger,
    ServiceFacade serviceFacade
) : IRequestHandler<RegisterRssJobUrlRequest, bool>
{
    public async Task<bool> Handle(RegisterRssJobUrlRequest request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Registering RSS Job. ChatId: {ChatId}, ThreadId: {ThreadId} RssUrl: {RssUrl}", request.ChatId, request.ThreadId, request.Url);

        RecurringJob.RemoveIfExists(request.JobId);
        RecurringJob.AddOrUpdate<RssScheduler>(
            request.JobId,
            methodCall: x => x.RssBroadcast(request.ChatId, request.ThreadId, request.Url),
            cronExpression: TimeUtil.MinuteInterval(3)
        );

        await Task.Delay(1, cancellationToken);

        return true;
    }
}
