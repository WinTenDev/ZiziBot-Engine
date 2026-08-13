using ZiziBot.Application.Common.Enums;

namespace ZiziBot.Application.Common.Dtos.Entity;

public class RssHistoryDto
{
    public long ChatId { get; set; }
    public int ThreadId { get; set; }
    public string RssUrl { get; set; }
    public string Url { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public DateTime PublishDate { get; set; }
    public EventStatus Status { get; set; }
}