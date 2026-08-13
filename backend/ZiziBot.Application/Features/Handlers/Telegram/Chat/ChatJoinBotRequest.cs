namespace ZiziBot.Application.Features.Handlers.Telegram.Chat;

public class ChatJoinBotRequest : BotRequestBase
{ }

public class ChatJoinBotHandler(
    ServiceFacade serviceFacade
) : IBotRequestHandler<ChatJoinBotRequest>
{
    public async Task<BotResponseBase> Handle(ChatJoinBotRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        await serviceFacade.TelegramService.AnswerJoinRequestAsync(request.ChatJoinRequest);


        return serviceFacade.TelegramService.Complete();
    }
}