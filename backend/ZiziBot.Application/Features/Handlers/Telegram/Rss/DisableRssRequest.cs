using Microsoft.EntityFrameworkCore;
using ZiziBot.Application.Features.Hangfire;

namespace ZiziBot.Application.Features.Handlers.Telegram.Rss;

public class DisableRssRequest : BotRequestBase
{
}

public class DisableRssHandler(
    DataFacade dataFacade,
    ServiceFacade serviceFacade
) : IBotRequestHandler<DisableRssRequest>
{
    public async Task<BotResponseBase> Handle(DisableRssRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        var rssUrl = request.Param;

        var rssSetting = await dataFacade.MongoDb.RssSetting.Where(x => x.RssUrl == rssUrl)
            .Where(x => x.Status == EventStatus.Complete || x.Status == EventStatus.InProgress)
            .FirstOrDefaultAsync();

        if (rssSetting == null)
        {
            return await serviceFacade.TelegramService.SendMessageAsync("RSS tidak ditemukan");
        }

        rssSetting.Status = EventStatus.Inactive;

        HangfireUtil.RemoveRecurringJob(rssSetting.CronJobId);

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        return await serviceFacade.TelegramService.SendMessageAsync("RSS Berhasil dimatikan");
    }
}