using Microsoft.Extensions.Logging;
using MongoFramework.Linq;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.Application.Handlers.Telegram.Chat;

public class GetSettingPanelBotRequestModel : BotRequestBase
{ }

public class GetSettingPanelRequestHandler(
    ILogger<GetSettingPanelRequestHandler> logger,
    DataFacade dataFacade,
    ServiceFacade serviceFacade
)
    : IRequestHandler<GetSettingPanelBotRequestModel, BotResponseBase>
{
    private readonly ILogger<GetSettingPanelRequestHandler> _logger = logger;

    public async Task<BotResponseBase> Handle(GetSettingPanelBotRequestModel request,
        CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        var chat = await dataFacade.MongoDb.ChatSetting.FirstOrDefaultAsync(x => x.ChatId == request.ChatIdentifier,
            cancellationToken);

        if (chat == null)
        {
            dataFacade.MongoDb.ChatSetting.Add(new ChatSettingEntity() {
                ChatId = request.ChatIdentifier,
            });

            await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);
        }

        await serviceFacade.TelegramService.SendMessageText("Sedang memuat tombol..");

        return serviceFacade.TelegramService.Complete();
    }
}