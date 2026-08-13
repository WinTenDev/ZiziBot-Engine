using Telegram.Bot;
using ZiziBot.Application.Common.Types;

namespace ZiziBot.Application.Features.Handlers.Telegram.Basic;

public class PingCallbackBotRequestModel : BotRequestBase
{
}

public class PingCallbackRequestHandler(
    ServiceFacade serviceFacade,
    DataFacade dataFacade
) : IBotRequestHandler<PingCallbackBotRequestModel>
{
    public async Task<BotResponseBase> Handle(PingCallbackBotRequestModel request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        if (!await dataFacade.ChatSetting.IsSudoAsync(request.UserId))
        {
            return await serviceFacade.TelegramService.AnswerCallbackAsync("Pong!");
        }

        var webhookInfo = await serviceFacade.TelegramService.Bot.GetWebhookInfo(cancellationToken: cancellationToken);
        var config = await dataFacade.AppSetting.GetConfigSectionAsync<EngineConfig>();

        var messageCallback = string.Empty;
        var htmlMessage = HtmlMessage.Empty;

        messageCallback += $"\nExecutionStrategy: {config?.ExecutionMode}";

        if (!webhookInfo.Url.IsNullOrEmpty())
        {
            messageCallback += "\nEngineMode: WebHook" +
                               "\nDetail WebHook info dikirimkan ke Private.";
        }
        else
        {
            messageCallback += "\nEngineMode: Polling";
            htmlMessage.Text(messageCallback);
        }

        await serviceFacade.TelegramService.AnswerCallbackAsync(messageCallback, showAlert: true);

        return await serviceFacade.TelegramService.SendMessageText(htmlMessage.ToString(), chatId: request.UserId);
    }
}