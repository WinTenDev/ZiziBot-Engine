using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Handlers.Telegram.Middleware;

public class CheckAntispamPipelineBehavior<TRequest, TResponse>(
    ILogger<CheckAntispamPipelineBehavior<TRequest, TResponse>> logger,
    ServiceFacade serviceFacade
)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : BotRequestBase
    where TResponse : BotResponseBase, new()
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        logger.LogDebug("Checking antispam for UserId: {UserId} in ChatId: {ChatId}", request.UserId, request.ChatId);

        var antiSpamDto = await serviceFacade.AntiSpamService.CheckSpamAsync(request.ChatIdentifier, request.UserId);

        request.PipelineResult.IsUserPassed = !antiSpamDto.IsBanAny;

        return await next();
    }
}