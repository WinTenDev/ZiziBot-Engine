using MongoDB.EntityFrameworkCore;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("RssSetting")]
public class RssSettingEntity : EntityBase
{
    public long ChatId { get; set; }
    public int? ThreadId { get; set; }
    public long UserId { get; set; }
    public string RssUrl { get; set; }
    public string? OriginalRssUrl { get; set; }
    public string? CronJobId { get; set; }
    public DateTime? LastSuccessDate { get; set; }
    public string? LastErrorMessage { get; set; }
}
