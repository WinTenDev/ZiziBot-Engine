﻿using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.Application.Handlers.Telegram.Basic;

public class InsertChatActivityHandler<TRequest, TResponse>(
    ILogger<InsertChatActivityHandler<TRequest, TResponse>> logger,
    TelegramService telegramService,
    MongoDbContextBase mongoDbContext
) : IRequestPostProcessor<TRequest, TResponse>
    where TRequest : BotRequestBase, IRequest<TResponse>
    where TResponse : BotResponseBase
{
    public async Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        logger.LogDebug("Insert Chat Activity for ChatId: {ChatId}", request.Chat);

        if (request.Source != ResponseSource.Bot)
            return;

        mongoDbContext.ChatActivity.Add(new ChatActivityEntity {
            MessageId = request.MessageId,
            ChatId = request.ChatIdentifier,
            ActivityType = ChatActivityType.UserSendMessage,
            Chat = request.Chat,
            User = request.User,
            Status = (int)EventStatus.Complete,
            TransactionId = request.TransactionId,
        });

        mongoDbContext.ChatActivity.Add(new ChatActivityEntity {
            MessageId = response.SentMessage.MessageId,
            ActivityType = ChatActivityType.BotSendMessage,
            ChatId = response.SentMessage.Chat.Id,
            Chat = response.SentMessage.Chat,
            User = response.SentMessage.From,
            Status = (int)EventStatus.Complete,
            TransactionId = request.TransactionId,
        });

        await mongoDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Insert Chat Activity for ChatId: {ChatId} is done", request.Chat);
    }
}