using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Infrastructure.Pipelines.PrePipeline;

public class CheckAntispamPipeline<TRequest, TResponse>(
    ILogger<CheckAntispamPipeline<TRequest, TResponse>> logger,
    ServiceFacade serviceFacade
) : ITelegramPreProcessPipeline<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<PreProcessResult<TResponse>> ProcessAsync(TRequest request, CancellationToken cancellationToken)
    {
        if (request is not BotRequestBase botRequest)
            return PreProcessResult<TResponse>.Continue;

        if (botRequest.Source != ResponseSource.Bot)
            return PreProcessResult<TResponse>.Continue;

        logger.LogDebug("Checking antispam for UserId: {UserId} in ChatId: {ChatId}", botRequest.UserId,
            botRequest.ChatId);

        var antiSpamDto =
            await serviceFacade.AntiSpamService.CheckSpamAsync(botRequest.ChatIdentifier, botRequest.UserId);

        botRequest.PipelineResult.IsUserPassed = !antiSpamDto.IsBanAny;

        return PreProcessResult<TResponse>.Continue;
    }
}
