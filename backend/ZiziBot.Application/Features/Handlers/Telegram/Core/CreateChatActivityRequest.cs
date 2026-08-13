using Microsoft.EntityFrameworkCore;
using Serilog;
using Telegram.Bot.Types;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Features.Handlers.Telegram.Core;

public class CreateChatActivityRequest : IAppCommand<object>, IRequest<object>
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
            Status = EventStatus.Complete,
            TransactionId = request.TransactionId,
            MessageId = request.SentMessage.MessageId
        });

        var oldActivity = await dataFacade.MongoDb.ChatActivity
                                    .Where(x => x.CreatedDate <= DateTime.UtcNow.AddMonths(-2))
                                    .ToListAsync(cancellationToken: cancellationToken);

        if (oldActivity.Count != 0)
        {
            Log.Information("Delete Chat Activity Count: {Count} in a 2 month", oldActivity.Count);
            dataFacade.MongoDb.ChatActivity.RemoveRange(oldActivity);
        }

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        return serviceFacade.TelegramService.Complete();
    }
}
