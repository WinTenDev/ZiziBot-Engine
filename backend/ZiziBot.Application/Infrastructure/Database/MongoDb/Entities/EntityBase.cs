using MongoDB.Bson;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

public class EntityBase
{
    public ObjectId Id { get; set; }
    public EventStatus Status { get; set; }
    public string? TransactionId { get; set; }
    public DateTime CreatedDate { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime UpdatedDate { get; set; }
    public long? UpdatedBy { get; set; }
}
