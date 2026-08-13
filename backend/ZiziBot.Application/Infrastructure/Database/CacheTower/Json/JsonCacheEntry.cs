namespace ZiziBot.Application.Infrastructure.Database.CacheTower.Json;

public struct JsonCacheEntry<TValue>
{
    public required string CacheKey { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ExpireDate { get; set; }
    public TValue? CacheValue { get; set; }
}
