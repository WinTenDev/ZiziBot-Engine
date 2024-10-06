namespace ZiziBot.Application.Handlers.Telegram.Chat;

public class PinMessageRequest : BotRequestBase
{ }

public class PinMessageHandler(
    ServiceFacade serviceFacade
) : IBotRequestHandler<PinMessageRequest>
{
    public async Task<BotResponseBase> Handle(PinMessageRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        if (request.ReplyToMessage == null)
        {
            return await serviceFacade.TelegramService.SendMessageAsync("Spesifikan pesan yang ingin dipin..");
        }

        await serviceFacade.TelegramService.SendMessageAsync("📍 Sedang mengepin pesan..");

        await serviceFacade.TelegramService.PinChatMessageAsync(request.ReplyToMessage.MessageId);

        return await serviceFacade.TelegramService.SendMessageAsync("📌 Pesan berhasil dipin");
    }
}