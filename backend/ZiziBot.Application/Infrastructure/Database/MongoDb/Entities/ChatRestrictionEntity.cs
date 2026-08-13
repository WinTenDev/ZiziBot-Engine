using MongoDB.EntityFrameworkCore;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("ChatRestriction")]
public class ChatRestrictionEntity : EntityBase
{
    public long UserId { get; set; }
    public long ChatId { get; set; }
}
