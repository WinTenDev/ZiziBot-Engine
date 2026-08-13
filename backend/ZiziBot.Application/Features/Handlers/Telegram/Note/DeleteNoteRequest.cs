namespace ZiziBot.Application.Features.Handlers.Telegram.Note;

public class DeleteNoteRequest : BotRequestBase
{
    public string Note { get; set; }
}

public class DeleteNoteRequestHandler(
    ServiceFacade serviceFacade,
    DataFacade dataFacade
)
    : IBotRequestHandler<DeleteNoteRequest>
{
    public async Task<BotResponseBase> Handle(DeleteNoteRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        await dataFacade.ChatSetting.Delete(request.ChatIdentifier, request.Note);
        await serviceFacade.TelegramService.SendMessageText("Note berhasil didelete");

        return serviceFacade.TelegramService.Complete();
    }
}