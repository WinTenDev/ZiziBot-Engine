using Microsoft.Extensions.Logging;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Infrastructure.Pipelines.PostPipeline;

public class InsertChatActivityPipeline<TRequest, TResponse>(
    ILogger<InsertChatActivityPipeline<TRequest, TResponse>> logger,
    DataFacade dataFacade
) : ITelegramPostProcessPipeline<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task ProcessAsync(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        if (request is not BotRequestBase botRequest)
            return;

        if (botRequest.Source != ResponseSource.Bot)
            return;

        logger.LogDebug("Insert Chat Activity for ChatId: {ChatId}", botRequest.ChatId);

        dataFacade.MongoDb.ChatActivity.Add(new ChatActivityEntity {
            MessageId = botRequest.MessageId,
            ChatId = botRequest.ChatIdentifier,
            UserId = botRequest.UserId,
            ActivityType = ChatActivityType.UserSendMessage,
            ActivityTypeName = ChatActivityType.UserSendMessage.ToString(),
            Status = EventStatus.Complete,
            TransactionId = botRequest.TransactionId,
        });

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Insert Chat Activity for ChatId: {ChatId} is done", botRequest.ChatId);
    }
}
