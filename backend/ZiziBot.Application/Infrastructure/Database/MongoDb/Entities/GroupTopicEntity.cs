using MongoDB.EntityFrameworkCore;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("GroupTopic")]
public class GroupTopicEntity : EntityBase
{
    public long ChatId { get; set; }
    public int? ThreadId { get; set; }
    public string? ThreadName { get; set; }
    public int IconColor { get; set; }
    public long IconCustomEmojiId { get; set; }
}
