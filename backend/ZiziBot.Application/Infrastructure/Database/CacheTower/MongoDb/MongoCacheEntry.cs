using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ZiziBot.Application.Infrastructure.Database.CacheTower.MongoDb;

internal struct MongoCacheEntry
{
    [BsonId]
    public ObjectId Id { get; set; }

    public required string CacheKey { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? Value { get; set; }
    public DateTime ExpiryDate { get; set; }
}
