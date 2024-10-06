namespace ZiziBot.Application.Handlers.Telegram.Note;

public class DeleteNoteRequest : BotRequestBase
{
    public string Note { get; set; }
}

public class DeleteNoteRequestHandler(
    ServiceFacade serviceFacade
) : IRequestHandler<DeleteNoteRequest, BotResponseBase>
{
    public async Task<BotResponseBase> Handle(DeleteNoteRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        return await serviceFacade.NoteService.Delete(request.ChatIdentifier, request.Note, async message => await serviceFacade.TelegramService.SendMessageText(message));
    }
}