using MongoDB.EntityFrameworkCore;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("GlobalBan")]
public class GlobalBanEntity : EntityBase
{
    public long UserId { get; set; }
    public long ChatId { get; set; }
    public string Reason { get; set; }
}
