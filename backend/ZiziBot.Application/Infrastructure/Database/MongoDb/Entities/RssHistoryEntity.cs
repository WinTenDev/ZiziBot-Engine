using MongoDB.EntityFrameworkCore;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("RssHistory")]
public class RssHistoryEntity : EntityBase
{
    public long ChatId { get; set; }
    public int? ThreadId { get; set; }
    public string? RssUrl { get; set; }
    public string? Url { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public DateTime PublishDate { get; set; }
    public string? Exception { get; set; }
    public string? ErrorMessage { get; set; }
    public HistoryType? HistoryType { get; set; }
    public string? HistoryTypeName { get; set; }
}
