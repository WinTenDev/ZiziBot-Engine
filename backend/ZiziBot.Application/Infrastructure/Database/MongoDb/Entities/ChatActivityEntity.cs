using MongoDB.EntityFrameworkCore;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("ChatActivity")]
public class ChatActivityEntity : EntityBase
{
    public long ChatId { get; set; }
    public long? UserId { get; set; }
    public int? MessageId { get; set; }
    public ChatActivityType? ActivityType { get; set; }
    public string? ActivityTypeName { get; set; }
}
