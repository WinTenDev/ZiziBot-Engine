using MongoDB.EntityFrameworkCore;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("ChannelPost")]
public class ChannelPostEntity : EntityBase
{
    public long SourceChannelId { get; set; }
    public long SourceMessageId { get; set; }
    public long DestinationChatId { get; set; }
    public long DestinationThreadId { get; set; }
    public long DestinationMessageId { get; set; }

    public string? Text { get; set; }
    public string? FileId { get; set; }
    public string? FileUniqueId { get; set; }
    public int MediaType { get; set; }
}
