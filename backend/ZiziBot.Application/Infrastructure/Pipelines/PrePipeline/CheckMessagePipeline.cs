using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Infrastructure.Pipelines.PrePipeline;

public class CheckMessagePipeline<TRequest, TResponse>(
    ILogger<CheckMessagePipeline<TRequest, TResponse>> logger,
    DataFacade dataFacade
) : ITelegramPreProcessPipeline<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<PreProcessResult<TResponse>> ProcessAsync(TRequest request, CancellationToken cancellationToken)
    {
        if (request is not BotRequestBase botRequest ||
            ShouldSkip(botRequest))
            return PreProcessResult<TResponse>.Continue;

        botRequest.ReplyMessage = true;

        var matchResult = await FindWordMatchAsync(botRequest);
        botRequest.PipelineResult.IsMessagePassed = !matchResult.HasBadword;
        botRequest.PipelineResult.Actions.AddRange(matchResult.Actions ?? []);

        return PreProcessResult<TResponse>.Continue;
    }

    private static bool ShouldSkip(BotRequestBase botRequest)
    {
        if (botRequest.IsChannel ||
            botRequest.Source == ResponseSource.Hangfire ||
            botRequest.IsPrivateChat ||
            botRequest.MessageTexts.IsEmpty())
            return true;

        return botRequest.RolesLevels.Any(x => x == RoleLevel.Sudo) &&
               botRequest.Command is "/dwf" or "/awf";
    }

    private async Task<MessageMatchResult> FindWordMatchAsync(BotRequestBase botRequest)
    {
        var words = await dataFacade.WordFilter.GetAllAsync();
        var messageTexts = botRequest.Message?.Text?.Explode() ?? [];

        foreach (var messageText in messageTexts)
        {
            var matchResult = FindWordMatch(botRequest, messageText, words);
            if (matchResult.HasBadword)
                return matchResult;
        }

        return MessageMatchResult.NoMatch;
    }

    private MessageMatchResult FindWordMatch(BotRequestBase botRequest, string messageText, IEnumerable<WordFilterDto> words)
    {
        foreach (var dto in words)
        {
            if (!messageText.Match(dto.Word))
                continue;

            logger.LogWarning("Check Message, pattern: {Pattern}, source: {MessageText}, action: {Action}, chatId: {ChatId}", dto.Word, messageText, dto.Action, botRequest.ChatId);

            return new MessageMatchResult(true, dto.Action);
        }

        return MessageMatchResult.NoMatch;
    }

    private sealed record MessageMatchResult(bool HasBadword, PipelineResultAction[]? Actions)
    {
        public static MessageMatchResult NoMatch { get; } = new(false, []);
    }
}