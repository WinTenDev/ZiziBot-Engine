using Hangfire;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Features.UseCases.Chat;

public class CreateChatActivityRequest
{
    public ChatActivityType ActivityType { get; set; }
    public long ChatId { get; set; }
    public int? ThreadId { get; set; }
    public int? MessageId { get; set; }
    public long? UserId { get; set; }
    public string TransactionId { get; set; }
}

public class CreateChatActivityUseCase(
    DataFacade dataFacade,
    ServiceFacade serviceFacade
)
{
    [Queue(CronJobKey.Queue_Telegram)]
    public async Task<bool> Handle(CreateChatActivityRequest request)
    {
        dataFacade.MongoDb.ChatActivity.Add(new ChatActivityEntity {
            ActivityType = request.ActivityType,
            ActivityTypeName = request.ActivityType.ToString(),
            ChatId = request.ChatId,
            UserId = request.UserId,
            Status = EventStatus.Complete,
            TransactionId = request.TransactionId,
            MessageId = request.MessageId
        });

        var oldActivity = await dataFacade.MongoDb.ChatActivity
            .Where(x => x.CreatedDate <= DateTime.UtcNow.AddMonths(-2))
            .ToListAsync();

        if (oldActivity.Count != 0)
        {
            Log.Information("Delete Chat Activity Count: {Count} in 2 months", oldActivity.Count);
            dataFacade.MongoDb.ChatActivity.RemoveRange(oldActivity);
        }

        await dataFacade.MongoDb.SaveChangesAsync();

        return true;
    }
}
