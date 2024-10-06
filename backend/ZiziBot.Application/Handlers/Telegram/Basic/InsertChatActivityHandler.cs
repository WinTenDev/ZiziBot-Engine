using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.Application.Handlers.Telegram.Basic;

public class InsertChatActivityHandler<TRequest, TResponse>(
    ILogger<InsertChatActivityHandler<TRequest, TResponse>> logger,
    DataFacade dataFacade
) : IRequestPostProcessor<TRequest, TResponse>
    where TRequest : BotRequestBase, IRequest<TResponse>
    where TResponse : BotResponseBase
{
    public async Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        logger.LogDebug("Insert Chat Activity for ChatId: {ChatId}", request.ChatId);

        if (request.Source != ResponseSource.Bot)
            return;

        dataFacade.MongoDb.ChatActivity.Add(new ChatActivityEntity {
            MessageId = request.MessageId,
            ChatId = request.ChatIdentifier,
            UserId = request.UserId,
            ActivityType = ChatActivityType.UserSendMessage,
            ActivityTypeName = ChatActivityType.UserSendMessage.ToString(),
            Chat = request.Chat,
            User = request.User,
            Status = (int)EventStatus.Complete,
            TransactionId = request.TransactionId,
        });

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Insert Chat Activity for ChatId: {ChatId} is done", request.ChatId);
    }
}