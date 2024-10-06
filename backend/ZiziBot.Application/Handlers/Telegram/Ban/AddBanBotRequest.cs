namespace ZiziBot.Application.Handlers.Telegram.Ban;

public class AddBanBotRequest : BotRequestBase
{ }

public class AddBanBotHandler(
    ServiceFacade serviceFacade,
    DataFacade dataFacade
)
    : IRequestHandler<AddBanBotRequest, BotResponseBase>
{
    public async Task<BotResponseBase> Handle(AddBanBotRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        var htmlMessage = HtmlMessage.Empty;
        var userId = request.MessageTexts?.Skip(1).FirstOrDefault().Convert<long>();
        var reason = request.Param.Replace(userId.ToString(), "");

        if (request.ReplyToMessage != null)
        {
            userId = request.ReplyToMessage.From?.Id;
        }

        if (userId == 0)
        {
            return await serviceFacade.TelegramService.SendMessageText("Spesifikasikan User yang ingin diblokir.");
        }

        await dataFacade.ChatSetting.SaveGlobalBan(new GlobalBanDto {
            UserId = userId.GetValueOrDefault(),
            ChatId = request.ChatIdentifier,
            Reason = reason
        });

        htmlMessage.Bold("Pengguna berhasi diban").Br()
            .Bold("UserID: ").CodeBr(userId.ToString());

        return await serviceFacade.TelegramService.SendMessageText(htmlMessage.ToString());
    }
}