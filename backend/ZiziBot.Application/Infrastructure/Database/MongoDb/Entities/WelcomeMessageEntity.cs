using MongoDB.EntityFrameworkCore;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("WelcomeMessage")]
public class WelcomeMessageEntity : EntityBase
{
    public long ChatId { get; set; }
    public long UserId { get; set; }
    public string? Text { get; set; }
    public string? RawButton { get; set; }
    public string? Media { get; set; }
    public CommonMediaType DataType { get; set; }
}
