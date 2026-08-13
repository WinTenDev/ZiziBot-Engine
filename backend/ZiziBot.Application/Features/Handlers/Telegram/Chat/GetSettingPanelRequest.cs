using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Features.Handlers.Telegram.Chat;

public class GetSettingPanelBotRequestModel : BotRequestBase
{ }

public class GetSettingPanelRequestHandler(
    ILogger<GetSettingPanelRequestHandler> logger,
    DataFacade dataFacade,
    ServiceFacade serviceFacade
)
    : IBotRequestHandler<GetSettingPanelBotRequestModel>
{
    public async Task<BotResponseBase> Handle(
        GetSettingPanelBotRequestModel request,
        CancellationToken cancellationToken
    )
    {
        serviceFacade.TelegramService.SetupResponse(request);

        var chat = await dataFacade.MongoDb.ChatSetting
            .Where(x => x.Status == EventStatus.Complete)
            .Where(x => x.ChatId == request.ChatIdentifier)
            .FirstOrDefaultAsync(cancellationToken);

        if (chat == null)
        {
            dataFacade.MongoDb.ChatSetting.Add(new ChatSettingEntity {
                ChatId = request.ChatIdentifier,
                Status = EventStatus.Complete,
            });

            await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);
        }

        return await serviceFacade.TelegramService.SendMessageText("Sedang memuat tombol..");
    }
}
