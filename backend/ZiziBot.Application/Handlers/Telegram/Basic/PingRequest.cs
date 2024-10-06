using Telegram.Bot.Types.ReplyMarkups;

namespace ZiziBot.Application.Handlers.Telegram.Basic;

public class PingBotRequestModel : BotRequestBase
{ }

public class PingRequestHandler(
    ServiceFacade serviceFacade
) : IRequestHandler<PingBotRequestModel, BotResponseBase>
{
    public async Task<BotResponseBase> Handle(PingBotRequestModel request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        var htmlMessage = HtmlMessage.Empty
            .BoldBr("Pong!")
            .Br();

        var replyMarkup = InlineKeyboardMarkup.Empty();

        if (await serviceFacade.SudoService.IsSudoAsync(request.UserId))
        {
            replyMarkup = new InlineKeyboardMarkup(
                new[] {
                    new[] {
                        new InlineKeyboardButton("WebHook Info") {
                            CallbackData = new PingCallbackQueryModel() {
                                Path = CallbackConst.BOT,
                                Data = "webhook-info"
                            }
                        }
                    }
                }
            );
        }

        return await serviceFacade.TelegramService.SendMessageText(htmlMessage.ToString(), replyMarkup);
    }
}