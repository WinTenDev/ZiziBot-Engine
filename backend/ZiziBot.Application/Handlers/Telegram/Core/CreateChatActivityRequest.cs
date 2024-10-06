using Telegram.Bot.Types;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.Application.Handlers.Telegram.Core;

public class CreateChatActivityRequest : IRequest<object>
{
    public ChatActivityType ActivityType { get; set; }
    public Message SentMessage { get; set; }
    public string TransactionId { get; set; }
}

public class CreateChatActivityHandler(
    DataFacade dataFacade,
    ServiceFacade serviceFacade
)
    : IRequestHandler<CreateChatActivityRequest, object>
{
    public async Task<object> Handle(CreateChatActivityRequest request, CancellationToken cancellationToken)
    {
        dataFacade.MongoDb.ChatActivity.Add(new ChatActivityEntity {
            ActivityType = request.ActivityType,
            ActivityTypeName = request.ActivityType.ToString(),
            ChatId = request.SentMessage.Chat.Id,
            UserId = request.SentMessage.From.Id,
            Chat = request.SentMessage.Chat,
            User = request.SentMessage.From,
            Status = (int)EventStatus.Complete,
            TransactionId = request.TransactionId,
            MessageId = request.SentMessage.MessageId
        });

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        return serviceFacade.TelegramService.Complete();
    }
}