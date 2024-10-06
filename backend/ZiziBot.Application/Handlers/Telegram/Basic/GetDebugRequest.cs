namespace ZiziBot.Application.Handlers.Telegram.Basic;

public class GetDebugBotRequest : BotRequestBase
{ }

public class GetDebugHandler(
    ServiceFacade serviceFacade
) : IBotRequestHandler<GetDebugBotRequest>
{
    public async Task<BotResponseBase> Handle(GetDebugBotRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        var htmlMessage = HtmlMessage.Empty
            .BoldBr("Debug Request")
            .Br();

        var message = request.Message;

        switch (serviceFacade.TelegramService.GetCommand())
        {
            case "/dbg":
                htmlMessage.CodeBr(message.ToYaml().HtmlDecode());
                break;
            case "/json":
                htmlMessage.CodeBr(message.ToJson(true).HtmlDecode());
                break;
        }

        return await serviceFacade.TelegramService.SendMessageText(htmlMessage.ToString());
    }
}