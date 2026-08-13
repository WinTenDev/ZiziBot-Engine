using CacheTower;
using MongoDB.Driver;
using Serilog;
using ZiziBot.Application.Infrastructure.Database.Utils;

namespace ZiziBot.Application.Infrastructure.Database.CacheTower.MongoDb;

internal class MongoLayerProvider(MongoLayerOptions options) : IDistributedCacheLayer
{
    private string DbName => options.ConnectionString.ToMongoUrl().DatabaseName;
    private MongoClient MongoClient => new MongoClient(options.ConnectionString);
    private IMongoDatabase MongoDatabase => MongoClient.GetDatabase(DbName);
    private IMongoCollection<MongoCacheEntry> Collection => MongoDatabase.GetCollection<MongoCacheEntry>("_cache");
    private readonly ILogger _log = Log.ForContext<MongoLayerProvider>();

    public async ValueTask FlushAsync()
    {
        _log.Verbose("Flush CacheTower MongoDB layer");
        await Collection.DeleteManyAsync(x => true);
        _log.Verbose("Flush CacheTower MongoDB layer has done");
    }

    public async ValueTask CleanupAsync()
    {
        _log.Verbose("Cleanup CacheTower MongoDB layer");
        await Collection.DeleteManyAsync(x => x.ExpiryDate < DateTime.UtcNow);
        _log.Verbose("Cleanup CacheTower MongoDB layer has done");
    }

    public async ValueTask EvictAsync(string cacheKey)
    {
        _log.Verbose("Evict CacheTower MongoDB layer. Key: {CacheKey}", cacheKey);
        await Collection.DeleteOneAsync(x => x.CacheKey == cacheKey);
        _log.Verbose("Evict CacheTower MongoDB layer has done. Key: {CacheKey}", cacheKey);
    }

    public async ValueTask<CacheEntry<T>?> GetAsync<T>(string cacheKey)
    {
        _log.Verbose("Get CacheTower MongoDB layer. Key: {CacheKey}", cacheKey);
        var cache = await Collection.Find(x => x.CacheKey == cacheKey).FirstOrDefaultAsync();
        var cacheEntry = default(CacheEntry<T>);

        cacheEntry = new(cache.Value.ToObject<T>(), cache.ExpiryDate);
        _log.Verbose("Get CacheTower MongoDB layer. Key: {CacheKey}, Expiry: {Expiry}", cacheKey, cache.ExpiryDate);

        return cacheEntry;
    }

    public async ValueTask SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry)
    {
        _log.Verbose("Set CacheTower MongoDB layer. Key: {CacheKey}", cacheKey);
        await Collection.UpdateOneAsync(
            filter: e => e.CacheKey == cacheKey,
            update: Builders<MongoCacheEntry>.Update
                .SetOnInsert(e => e.CreatedDate, DateTime.UtcNow)
                .Set(x => x.Value, cacheEntry.Value.ToJson())
                .Set(x => x.ExpiryDate, cacheEntry.Expiry),
            options: new UpdateOptions {
                IsUpsert = true
            }
        );

        _log.Verbose("Set CacheTower MongoDB layer. Key: {CacheKey}", cacheKey);
    }

    public async ValueTask<bool> IsAvailableAsync(string cacheKey)
    {
        var documentCount = await Collection.EstimatedDocumentCountAsync();

        var isAvailable = documentCount >= 0;
        _log.Verbose("CacheTower MongoDB layer is available: {IsAvailable}", isAvailable);

        return isAvailable;
    }
}
