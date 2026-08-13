using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Infrastructure.Pipelines.PostPipeline;

public class CheckAfkSessionPipeline<TRequest, TResponse>(
    ILogger<CheckAfkSessionPipeline<TRequest, TResponse>> logger,
    DataFacade dataFacade,
    ServiceFacade serviceFacade
) : ITelegramPostProcessPipeline<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task ProcessAsync(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        if (request is not BotRequestBase botRequest)
            return;

        if (botRequest.Source != ResponseSource.Bot ||
            botRequest.IsChannel)
            return;

        botRequest.ReplyMessage = true;
        botRequest.DeleteAfter = TimeSpan.FromMinutes(1);
        botRequest.CleanupTargets = new[] {
            CleanupTarget.FromBot
        };

        serviceFacade.TelegramService.SetupResponse(botRequest);

        if (serviceFacade.TelegramService.IsCommand("/afk"))
            return;

        logger.LogInformation("CheckAfkSessionBehavior Started");

        var userId = botRequest.UserId;
        var userName = botRequest.User.GetFullMention();

        if (botRequest.ReplyToMessage != null)
        {
            userId = botRequest.ReplyToMessage.From.Id;
            userName = botRequest.ReplyToMessage.From.GetFullMention();
        }

        var afkEntity = await dataFacade.MongoDb.Afk
            .FirstOrDefaultAsync(entity =>
                    entity.UserId == userId &&
                    entity.Status == EventStatus.Complete,
                cancellationToken);

        if (afkEntity == null)
            return;

        if (userId == botRequest.UserId)
        {
            await serviceFacade.TelegramService.SendMessageText($"{userName} sudah tidak AFK");
            afkEntity.Status = EventStatus.Deleted;
        }
        else
        {
            await serviceFacade.TelegramService.SendMessageText($"{userName} sedang AFK");
        }

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);
    }
}
