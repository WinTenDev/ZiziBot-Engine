using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Infrastructure.Pipelines.PrePipeline;

public class CheckUsernamePipeline<TRequest, TResponse>(
    ILogger<CheckUsernamePipeline<TRequest, TResponse>> logger,
    ServiceFacade serviceFacade
) : ITelegramPreProcessPipeline<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<PreProcessResult<TResponse>> ProcessAsync(TRequest request, CancellationToken cancellationToken)
    {
        if (request is not BotRequestBase botRequest)
            return PreProcessResult<TResponse>.Continue;

        if (botRequest.Source != ResponseSource.Bot ||
            botRequest.ChannelPostAny != null)
            return PreProcessResult<TResponse>.Continue;

        serviceFacade.TelegramService.SetupResponse(botRequest);

        logger.LogDebug("Checking Username for UserId: {UserId} in ChatId: {ChatId}", botRequest.UserId,
            botRequest.ChatId);

        if (!await serviceFacade.TelegramService.UserHasUsername())
        {
            botRequest.PipelineResult.Actions.AddRange([
                PipelineResultAction.Mute,
                PipelineResultAction.Warn
            ]);

            return PreProcessResult<TResponse>.Continue;
        }

        logger.LogDebug("User passed from checking Username for UserId: {UserId} in ChatId: {ChatId}",
            botRequest.UserId, botRequest.ChatId);

        botRequest.PipelineResult.HasUsername = true;

        return PreProcessResult<TResponse>.Continue;
    }
}
