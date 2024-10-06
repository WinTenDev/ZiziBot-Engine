namespace ZiziBot.Application.Handlers.Telegram.Chat;

public class ChatJoinBotRequest : BotRequestBase
{ }

public class ChatJoinBotHandler(
    ServiceFacade serviceFacade
) : IRequestHandler<ChatJoinBotRequest, BotResponseBase>
{
    public async Task<BotResponseBase> Handle(ChatJoinBotRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        await serviceFacade.TelegramService.AnswerJoinRequestAsync(request.ChatJoinRequest);


        return serviceFacade.TelegramService.Complete();
    }
}