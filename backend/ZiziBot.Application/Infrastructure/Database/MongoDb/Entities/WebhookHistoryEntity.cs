using MongoDB.EntityFrameworkCore;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("WebhookHistory")]
public class WebhookHistoryEntity : EntityBase
{
    public required string RouteId { get; set; }
    public long ChatId { get; set; }
    public int MessageId { get; set; }
    public int? MessageThreadId { get; set; }
    public WebhookSource WebhookSource { get; set; }
    public string? UserAgent { get; set; }
    public string? WebhookSourceName { get; set; }
    public string? Header { get; set; }
    public required string EventName { get; set; }
    public string? Payload { get; set; }
    public TimeSpan Elapsed { get; set; }
    public bool? IsDebug { get; set; }
}
