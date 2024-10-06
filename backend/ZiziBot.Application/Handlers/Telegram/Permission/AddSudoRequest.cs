using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.Application.Handlers.Telegram.Permission;

public class AddSudoBotRequestModel : BotRequestBase
{
    public long CustomUserId { get; set; }
}

[UsedImplicitly]
public class AddSudoRequestHandler(
    DataFacade dataFacade,
    ServiceFacade serviceFacade
) : IRequestHandler<AddSudoBotRequestModel, BotResponseBase>
{
    public async Task<BotResponseBase> Handle(AddSudoBotRequestModel request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        await serviceFacade.TelegramService.SendMessageText("Adding sudo user...");

        var serviceResult = await serviceFacade.SudoService.SaveSudo(new SudoerEntity() {
            UserId = request.CustomUserId == 0 ?
                request.UserId :
                request.CustomUserId,
            PromotedBy = request.UserId,
            PromotedFrom = request.ChatIdentifier,
            Status = (int)EventStatus.Complete
        });

        await serviceFacade.TelegramService.EditMessageText(serviceResult.Message);

        return serviceFacade.TelegramService.Complete();
    }
}