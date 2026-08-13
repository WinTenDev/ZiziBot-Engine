using MongoDB.EntityFrameworkCore;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("WebhookChat")]
public class WebhookChatEntity : EntityBase
{
    public string RouteId { get; set; }
    public long ChatId { get; set; }
    public int? MessageThreadId { get; set; }
    public bool? IsDebug { get; set; }
}
