using Telegram.Bot.Types.ReplyMarkups;
using ZiziBot.Application.Common.Types;

namespace ZiziBot.Application.Features.Handlers.Telegram.Basic;

[FeatureFlag(Flag.COMMAND_PING)]
public class PingBotRequestModel : BotRequestBase
{ }

public class PingRequestHandler(
    ServiceFacade serviceFacade,
    DataFacade dataFacade
) : IBotRequestHandler<PingBotRequestModel>
{
    public async Task<BotResponseBase> Handle(PingBotRequestModel request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        var htmlMessage = HtmlMessage.Empty
            .BoldBr("Pong!")
            .Br();

        var replyMarkup = InlineKeyboardMarkup.Empty();

        if (await dataFacade.ChatSetting.IsSudoAsync(request.UserId))
        {
            replyMarkup = new InlineKeyboardMarkup([
                [
                    new InlineKeyboardButton("Ping!") {
                        CallbackData = "ping"
                    }
                ]
            ]);
        }

        return await serviceFacade.TelegramService.SendMessageAsync(htmlMessage.ToString(), replyMarkup);
    }
}