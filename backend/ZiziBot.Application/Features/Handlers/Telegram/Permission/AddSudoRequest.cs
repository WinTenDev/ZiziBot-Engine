using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Features.Handlers.Telegram.Permission;

public class AddSudoBotRequestModel : BotRequestBase
{
    public long CustomUserId { get; set; }
}

[UsedImplicitly]
public class AddSudoRequestHandler(
    DataFacade dataFacade,
    ServiceFacade serviceFacade
) : IBotRequestHandler<AddSudoBotRequestModel>
{
    public async Task<BotResponseBase> Handle(AddSudoBotRequestModel request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        await serviceFacade.TelegramService.SendMessageText("Adding sudo user...");

        var serviceResult = await dataFacade.ChatSetting.SaveSudo(new SudoerEntity() {
            UserId = request.CustomUserId == 0 ? request.UserId : request.CustomUserId,
            PromotedBy = request.UserId,
            PromotedFrom = request.ChatIdentifier,
            Status = EventStatus.Complete,
            TransactionId = request.TransactionId
        });

        await serviceFacade.TelegramService.EditMessageText(serviceResult.Message);

        return serviceFacade.TelegramService.Complete();
    }
}
