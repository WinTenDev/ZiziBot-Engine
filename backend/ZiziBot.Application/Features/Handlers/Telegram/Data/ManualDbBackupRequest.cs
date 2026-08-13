namespace ZiziBot.Application.Features.Handlers.Telegram.Data;

public class ManualDbBackupRequest : BotRequestBase
{ }

public class ManualDbBackupHandler(ServiceFacade serviceFacade) : IBotRequestHandler<ManualDbBackupRequest>
{
    public async Task<BotResponseBase> Handle(ManualDbBackupRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        _ = await serviceFacade.MediatorService.Send(new MongoDbBackupRequest());

        return await serviceFacade.TelegramService.SendMessageText("Backup database berhasil");
    }
}