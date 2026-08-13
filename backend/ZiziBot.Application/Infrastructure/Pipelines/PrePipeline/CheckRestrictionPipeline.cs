namespace ZiziBot.Application.Infrastructure.Pipelines.PrePipeline;

public class CheckRestrictionPipeline<TRequest, TResponse>(
    ServiceFacade serviceFacade,
    DataFacade dataFacade
) : ITelegramPreProcessPipeline<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<PreProcessResult<TResponse>> ProcessAsync(TRequest request, CancellationToken cancellationToken)
    {
        if (request is not BotRequestBase botRequest)
            return PreProcessResult<TResponse>.Continue;

        if (botRequest.IsChannel ||
            botRequest.InlineQuery != null ||
            botRequest.CallbackQuery != null ||
            botRequest.IsPrivateChat ||
            botRequest.Source != ResponseSource.Bot)
            return PreProcessResult<TResponse>.Continue;

        var config = await dataFacade.AppSetting.GetConfigSectionAsync<EngineConfig>();

        if (!(config?.EnableChatRestriction ?? false))
            return PreProcessResult<TResponse>.Continue;

        serviceFacade.TelegramService.SetupResponse(botRequest);

        var check = await dataFacade.ChatSetting.GetChatRestriction(botRequest.ChatIdentifier);

        botRequest.PipelineResult.IsChatPassed = check != null;

        return PreProcessResult<TResponse>.Continue;
    }
}
