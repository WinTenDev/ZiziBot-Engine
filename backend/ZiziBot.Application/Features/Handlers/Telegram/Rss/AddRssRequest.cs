using Microsoft.Extensions.Logging;
using ZiziBot.Application.Features.UseCases.Rss;

namespace ZiziBot.Application.Features.Handlers.Telegram.Rss;

public class AddRssRequest : BotRequestBase
{
}

public class AddRssHandler(
    ILogger<AddRssHandler> logger,
    DataFacade dataFacade,
    ServiceFacade serviceFacade,
    AddRssUseCase addRssUseCase
)
    : IBotRequestHandler<AddRssRequest>
{
    public async Task<BotResponseBase> Handle(AddRssRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        if (!await dataFacade.FeatureFlag.IsEnabled(Flag.RSS_BROADCASTER))
            return await serviceFacade.TelegramService.SendMessageAsync("Fitur RSS saat ini sedang dimatikan");

        if (request.Param.IsNullOrEmpty())
            return await serviceFacade.TelegramService.SendMessageAsync("Masukkan RSS URL yang ingin ditambahkan");

        await serviceFacade.TelegramService.SendMessageAsync("🔍 Sedang memeriksa RSS..");

        try
        {
            var rssUrl = await addRssUseCase.Handle(new AddRssParam() {
                ChatId = request.ChatIdentifier,
                UserId = request.UserId,
                ThreadId = request.MessageThreadId,
                Url = request.Param
            });

            return await serviceFacade.TelegramService.SendMessageAsync("🔖 RSS Feed" +
                                                                        "\nURL: " + rssUrl);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed when adding RSS Url: {Url} to ChatId: {ChatId}", request.Param, request.ChatIdentifier);
            return await serviceFacade.TelegramService.SendMessageAsync("Terjadi sesuatu ketika menambahkan RSS");
        }
    }
}