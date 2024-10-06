using Hangfire;
using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Handlers.Telegram.Rss;

public class RegisterRssJobUrlRequest : IRequest<bool>
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

        serviceFacade.RecurringJobManager.RemoveIfExists(request.JobId);
        serviceFacade.RecurringJobManager.AddOrUpdate<MediatorService>(
            recurringJobId: request.JobId,
            methodCall: mediatorService => mediatorService.Send(new FetchRssRequest() {
                ChatId = request.ChatId,
                ThreadId = request.ThreadId,
                RssUrl = request.Url
            }),
            queue: CronJobKey.Queue_Rss,
            cronExpression: TimeUtil.MinuteInterval(3));

        await Task.Delay(1);

        return true;
    }
}