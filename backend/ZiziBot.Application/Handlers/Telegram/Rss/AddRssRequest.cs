using MongoFramework.Linq;

namespace ZiziBot.Application.Handlers.Telegram.Rss;

public class AddRssRequest : BotRequestBase
{ }

public class AddRssHandler(
    DataFacade dataFacade,
    ServiceFacade serviceFacade
) : IRequestHandler<AddRssRequest, BotResponseBase>
{
    public async Task<BotResponseBase> Handle(AddRssRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        if (request.Param.IsNullOrEmpty())
        {
            return await serviceFacade.TelegramService.SendMessageAsync("Masukkan RSS URL yang ingin ditambahkan");
        }

        var rssUrl = request.Param.TryFixRssUrl();

        try
        {
            await serviceFacade.TelegramService.SendMessageAsync("Sedang memverifikasi URL");
            var feed = await rssUrl.ReadRssAsync();
        }
        catch (Exception e)
        {
            return await serviceFacade.TelegramService.SendMessageAsync("Sepertinya bukan URL yang valid");
        }

        var rssSetting = await dataFacade.MongoDb.RssSetting
            .Where(entity => entity.RssUrl == rssUrl)
            .Where(entity => entity.ChatId == request.ChatIdentifier)
            .Where(entity => entity.ThreadId == request.MessageThreadId)
            .Where(entity => entity.Status == (int)EventStatus.Complete)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (rssSetting != null)
            return await serviceFacade.TelegramService.SendMessageAsync("RSS Sudah disimpan");

        var uniqueId = await StringUtil.GetNanoIdAsync(prefix: "RssJob:", size: 7);

        dataFacade.MongoDb.RssSetting.Add(new() {
            ChatId = request.ChatIdentifier,
            RssUrl = rssUrl,
            ThreadId = request.MessageThreadId,
            UserId = request.UserId,
            CronJobId = uniqueId,
            Status = (int)EventStatus.Complete
        });

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        await serviceFacade.TelegramService.SendMessageAsync("Membuat Cron Job");

        await serviceFacade.Mediator.Send(new RegisterRssJobUrlRequest {
            ChatId = request.ChatIdentifier,
            ThreadId = request.MessageThreadId,
            Url = rssUrl,
            JobId = uniqueId
        }, cancellationToken);

        return await serviceFacade.TelegramService.SendMessageAsync("RSS Berhasil disimpan");
    }
}