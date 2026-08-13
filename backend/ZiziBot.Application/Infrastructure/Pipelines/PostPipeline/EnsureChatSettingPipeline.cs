using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Infrastructure.Pipelines.PostPipeline;

public class EnsureChatSettingPipeline<TRequest, TResponse>(
    ILogger<EnsureChatSettingPipeline<TRequest, TResponse>> logger,
    DataFacade dataFacade,
    ServiceFacade serviceFacade
) : ITelegramPostProcessPipeline<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task ProcessAsync(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        if (request is not BotRequestBase botRequest)
            return;

        if (botRequest.ChatId == 0 ||
            botRequest.Source != ResponseSource.Bot)
            return;

        serviceFacade.TelegramService.SetupResponse(botRequest);

        var memberCount = await serviceFacade.TelegramService.GetMemberCount();
        var isBotAdmin = await serviceFacade.TelegramService.CheckBotAdmin();

        await dataFacade.ChatSetting.RefreshChatInfo(new() {
            ChatId = botRequest.ChatIdentifier,
            ChatType = botRequest.ChatType,
            MemberCount = botRequest.IsGroup ? memberCount : -1,
            ChatTitle = botRequest.ChatTitle,
            ChatUsername = botRequest.Chat?.Username,
            IsBotAdmin = isBotAdmin,
            TransactionId = botRequest.TransactionId
        });

        logger.LogInformation("Ensure ChatSetting for ChatId: {ChatId} Done", botRequest.ChatId);
    }
}
