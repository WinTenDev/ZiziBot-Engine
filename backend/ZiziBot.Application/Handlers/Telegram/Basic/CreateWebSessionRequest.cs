using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ZiziBot.Application.Handlers.Telegram.Basic;

public class CreateWebSessionBotRequestModel : BotRequestBase
{
}

public class CreateWebSessionRequestHandler : IRequestHandler<CreateWebSessionBotRequestModel, BotResponseBase>
{
    private readonly TelegramService _telegramService;

    public CreateWebSessionRequestHandler(TelegramService telegramService)
    {
        _telegramService = telegramService;
    }

    public async Task<BotResponseBase> Handle(CreateWebSessionBotRequestModel request, CancellationToken cancellationToken)
    {
        _telegramService.SetupResponse(request);

        var sessionId = Guid.NewGuid().ToString();
        var webUrlBase = Env.WEB_VERIFY_SESSION_URL + "?session_id=";
        var webUrl = webUrlBase + sessionId;

        if (!EnvUtil.IsEnvExist(Env.WEB_CONSOLE_URL))
        {
            await _telegramService.SendMessageText("Maaf fitur ini belum dipersiapkan");
        }

        var htmlMessage = HtmlMessage.Empty
            .BoldBr("🎛 ZiziBot Console")
            .TextBr("Silakan klik tombol dibawah ini untuk membuka.")
            .Br();

        if (webUrl.Contains("localhost"))
        {
            htmlMessage.Code(webUrl).Br();
        }

        var replyMarkup = InlineKeyboardMarkup.Empty();
        if (!webUrl.Contains("localhost"))
        {
            replyMarkup = new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithLoginUrl("Buka Console", new LoginUrl()
                    {
                        Url = webUrl
                    })
                }
            }.ToButtonMarkup();
        }

        return await _telegramService.SendMessageText(htmlMessage.ToString(), replyMarkup);
    }
}