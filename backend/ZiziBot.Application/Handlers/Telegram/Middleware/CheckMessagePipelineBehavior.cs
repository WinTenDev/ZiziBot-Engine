using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Handlers.Telegram.Middleware;

public class CheckMessagePipelineBehavior<TRequest, TResponse>(
    ILogger<CheckMessagePipelineBehavior<TRequest, TResponse>> logger,
    ServiceFacade serviceFacade,
    DataFacade dataFacade
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : BotRequestBase
    where TResponse : BotResponseBase, new()
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request.IsChannel ||
            request.Source == ResponseSource.Hangfire ||
            request.IsPrivateChat)
            return await next();

        if (request.MessageTexts.IsEmpty())
            return await next();

        if (request.RolesLevels.Any(x => x == RoleLevel.Sudo))
        {
            if (request.Command is "/dwf" or "/awf")
            {
                return await next();
            }
        }

        request.ReplyMessage = true;

        var hasBadword = false;
        var matchPattern = string.Empty;
        PipelineResultAction[]? action = [];

        var words = await dataFacade.WordFilter.GetAllAsync();

        var messageTexts = request.Message?.Text?.Explode();

        foreach (var messageText in messageTexts)
        {
            foreach (var dto in words)
            {
                var pattern = dto.Word;

                hasBadword = messageText.Match(pattern);

                if (!hasBadword)
                    continue;

                logger.LogWarning("Check message pattern: {Pattern}, source: {MessageText}, action: {Action}", pattern, messageText, dto.Action);

                matchPattern = dto.Word;
                action = dto.Action;

                break;
            }

            if (hasBadword)
                break;
        }

        request.PipelineResult.IsMessagePassed = !hasBadword;
        request.PipelineResult.Actions.AddRange(action ?? []);

        return await next();
    }
}