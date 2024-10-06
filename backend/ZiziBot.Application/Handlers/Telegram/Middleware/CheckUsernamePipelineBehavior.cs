using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Handlers.Telegram.Middleware;

public class CheckUsernamePipelineBehavior<TRequest, TResponse>(
    ILogger<CheckUsernamePipelineBehavior<TRequest, TResponse>> logger,
    ServiceFacade serviceFacade
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : BotRequestBase, IRequest<TResponse>
    where TResponse : BotResponseBase, new()
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (request.Source != ResponseSource.Bot ||
            request.ChannelPostAny != null)
            return await next();

        serviceFacade.TelegramService.SetupResponse(request);

        logger.LogDebug("Checking Username for UserId: {UserId} in ChatId: {ChatId}", request.UserId, request.ChatId);

        if (!await serviceFacade.TelegramService.UserHasUsername())
        {
            request.PipelineResult.Actions.AddRange(new[] {
                PipelineResultAction.Mute,
                PipelineResultAction.Warn
            });

            return await next();
        }

        logger.LogDebug("User passed from checking Username for UserId: {UserId} in ChatId: {ChatId}", request.UserId, request.ChatId);

        request.PipelineResult.HasUsername = true;

        return await next();
    }
}