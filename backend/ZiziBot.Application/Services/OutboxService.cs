using System.Text.Json;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;
using ZiziBot.Application.Infrastructure.Database.Service;

namespace ZiziBot.Application.Services;

public class OutboxService(DataFacade dataFacade)
{
    public async Task EnqueueAsync<TPayload>(
        string type,
        TPayload payload,
        string? transactionId = null,
        CancellationToken cancellationToken = default
    )
    {
        var entity = new OutboxMessageEntity
        {
            Type = type,
            Payload = JsonSerializer.Serialize(payload),
            OccurredAt = DateTime.UtcNow,
            ProcessedAt = null,
            Attempts = 0,
            Error = null,
            Status = EventStatus.Complete,
            TransactionId = transactionId
        };

        await dataFacade.Outbox.EnqueueAsync(entity, cancellationToken);
    }

    public async Task EnqueueAndSaveAsync<TPayload>(
        string type,
        TPayload payload,
        string? transactionId = null,
        CancellationToken cancellationToken = default
    )
    {
        await EnqueueAsync(type, payload, transactionId, cancellationToken);
        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);
    }
}
