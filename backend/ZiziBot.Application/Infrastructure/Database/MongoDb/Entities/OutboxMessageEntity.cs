using MongoDB.EntityFrameworkCore;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("OutboxMessage")]
public class OutboxMessageEntity : EntityBase
{
    public string Type { get; set; }
    public string Payload { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int Attempts { get; set; }
    public string? Error { get; set; }
}
