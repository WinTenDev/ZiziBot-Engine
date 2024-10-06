namespace ZiziBot.Application.Handlers.Telegram.Middleware;

public class CheckRestrictionPipelineBehavior<TRequest, TResponse>(
    ServiceFacade serviceFacade,
    DataFacade dataFacade
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : BotRequestBase
    where TResponse : BotResponseBase, new()
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request.IsChannel ||
            request.InlineQuery != null ||
            request.CallbackQuery != null ||
            request.IsPrivateChat ||
            request.Source != ResponseSource.Bot)
            return await next();

        var config = await dataFacade.AppSetting.GetConfigSectionAsync<EngineConfig>();

        if (!(config?.EnableChatRestriction ?? false))
            return await next();

        serviceFacade.TelegramService.SetupResponse(request);

        var check = await dataFacade.ChatSetting.GetChatRestriction(request.ChatIdentifier);

        request.PipelineResult.IsChatPassed = check != null;

        return await next();
    }
}