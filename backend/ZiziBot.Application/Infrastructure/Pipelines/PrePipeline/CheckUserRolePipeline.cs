using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Infrastructure.Pipelines.PrePipeline;

public class CheckUserRolePipeline<TRequest, TResponse>(
    ILogger<CheckUserRolePipeline<TRequest, TResponse>> logger,
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

        logger.LogDebug("Checking Role {Name} for UserId: {UserId} in ChatId: {ChatId}", typeof(TRequest), botRequest.UserId, botRequest.ChatId);

        serviceFacade.TelegramService.SetupResponse(botRequest);

        var isRoleMeet = await serviceFacade.TelegramService.ValidateRole();

        if (botRequest.MinimumRole > RoleLevel.None)
            logger.LogWarning("The minimum role for {Name} for UserId: {UserId} in ChatId: {ChatId} should have role minimum {Role}? {Result}",
                typeof(TRequest).Name, botRequest.UserId, botRequest.ChatId, botRequest.MinimumRole, isRoleMeet);

        botRequest.PipelineResult.IsRolePassed = isRoleMeet;

        return PreProcessResult<TResponse>.Continue;
    }
}
