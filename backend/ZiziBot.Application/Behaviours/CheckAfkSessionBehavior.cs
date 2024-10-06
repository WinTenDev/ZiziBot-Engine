using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using MongoFramework.Linq;

namespace ZiziBot.Application.Behaviours;

public class CheckAfkSessionBehavior<TRequest, TResponse>(
    ILogger<CheckAfkSessionBehavior<TRequest, TResponse>> logger,
    DataFacade dataFacade,
    ServiceFacade serviceFacade
)
    : IRequestPostProcessor<TRequest, TResponse>
    where TRequest : BotRequestBase, IRequest<TResponse>
    where TResponse : BotResponseBase
{
    public async Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        if (request.Source != ResponseSource.Bot ||
            request.IsChannel)
            return;

        request.ReplyMessage = true;
        request.DeleteAfter = TimeSpan.FromMinutes(1);
        request.CleanupTargets = new[] {
            CleanupTarget.FromBot
        };


        serviceFacade.TelegramService.SetupResponse(request);

        if (serviceFacade.TelegramService.IsCommand("/afk"))
            return;

        logger.LogInformation("CheckAfkSessionBehavior Started");

        var userId = request.UserId;
        var userName = request.User.GetFullMention();

        if (request.ReplyToMessage != null)
        {
            userId = request.ReplyToMessage.From.Id;
            userName = request.ReplyToMessage.From.GetFullMention();
        }

        var afkEntity = await dataFacade.MongoDb.Afk
            .FirstOrDefaultAsync(entity =>
                    entity.UserId == userId &&
                    entity.Status == (int)EventStatus.Complete,
                cancellationToken);

        if (afkEntity == null)
            return;

        if (userId == request.UserId)
        {
            await serviceFacade.TelegramService.SendMessageText($"{userName} sudah tidak AFK");
            afkEntity.Status = (int)EventStatus.Deleted;
        }
        else
        {
            await serviceFacade.TelegramService.SendMessageText($"{userName} sedang AFK");
        }


        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);
    }
}