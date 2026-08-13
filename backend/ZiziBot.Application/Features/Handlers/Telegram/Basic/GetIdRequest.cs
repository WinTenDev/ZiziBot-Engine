using Telegram.Bot.Types.Enums;
using ZiziBot.Application.Common.Types;

namespace ZiziBot.Application.Features.Handlers.Telegram.Basic;

public class GetIdBotRequestModel : BotRequestBase
{ }

public class GetIdRequestHandler(
    ServiceFacade serviceFacade
) : IBotRequestHandler<GetIdBotRequestModel>
{
    public async Task<BotResponseBase> Handle(GetIdBotRequestModel request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        var htmlMessage = HtmlMessage.Empty;

        if (request.ChatType != ChatType.Private)
        {
            htmlMessage.BoldBr($"👥 {request.ChatTitle}")
                .Bold("Chat ID: ").CodeBr(request.ChatId.ToString());
        }

        if (request.Message?.IsTopicMessage ?? false)
        {
            htmlMessage.Br()
                .Bold("🧵 ").BoldBr(request.ReplyToMessage?.ForumTopicCreated?.Name)
                .Bold("Topic ID: ").CodeBr(request.ReplyToMessage?.MessageThreadId.ToString());
        }

        htmlMessage.Br()
            .BoldBr($"👤 {request.UserFullName}")
            .Bold("User ID: ").CodeBr(request.UserId.ToString());

        return await serviceFacade.TelegramService.SendMessageText(htmlMessage.ToString());
    }
}