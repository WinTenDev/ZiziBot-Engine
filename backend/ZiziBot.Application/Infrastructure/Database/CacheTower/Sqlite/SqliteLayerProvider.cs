using CacheTower;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ZiziBot.Application.Infrastructure.Database.Sqlite;
using ZiziBot.Application.Infrastructure.Database.Sqlite.Entities;

namespace ZiziBot.Application.Infrastructure.Database.CacheTower.Sqlite;

internal class SqliteLayerProvider : ICacheLayer
{
    private readonly string _dbPath;
    private SqliteCachingDbContext Db => new SqliteCachingDbContext(_dbPath);
    private readonly ILogger _log = Log.ForContext<SqliteLayerProvider>();

    public SqliteLayerProvider(string dbPath)
    {
        _dbPath = dbPath;

        Db.Database.EnsureCreated();
        _log.Verbose("Table structure for CacheTower sqlite created");
    }

    public async ValueTask FlushAsync()
    {
        _log.Verbose("Flush CacheTower sqlite layer");
        await Db.SqliteCache.ExecuteDeleteAsync();
        // await Db.SaveChangesAsync();
        _log.Verbose("Flush CacheTower sqlite layer has done");
    }

    public async ValueTask CleanupAsync()
    {
        _log.Verbose("Cleanup CacheTower sqlite layer");
        await Db.SqliteCache.Where(x => x.ExpiryDate < DateTime.UtcNow).ExecuteDeleteAsync();
        // await Db.SaveChangesAsync();
        _log.Verbose("Cleanup CacheTower sqlite layer done");
    }

    public async ValueTask EvictAsync(string cacheKey)
    {
        _log.Verbose("Evict CacheTower sqlite layer. Key: {CacheKey}", cacheKey);
        await Db.SqliteCache.Where(x => x.CacheKey == cacheKey).ExecuteDeleteAsync();
        // await Db.SaveChangesAsync();
        _log.Verbose("Evict CacheTower sqlite layer has done. Key: {CacheKey}", cacheKey);
    }

    public async ValueTask<CacheEntry<T>?> GetAsync<T>(string cacheKey)
    {
        _log.Verbose("Get CacheTower sqlite layer. Key: {CacheKey}", cacheKey);
        var cacheEntry = default(CacheEntry<T>);

        var obj = await Db.SqliteCache.AsNoTracking()
            .Where(x => x.CacheKey == cacheKey)
            .FirstOrDefaultAsync();

        if (obj != null)
        {
            var value = obj.Value.ToObject<T>();
            var cache = new CacheEntry<T>(value, obj.ExpiryDate);
            _log.Verbose("Get CacheTower sqlite layer. Key: {CacheKey}, Expiry: {Expiry}", cacheKey, obj.ExpiryDate);

            return cache;
        }

        return cacheEntry;
    }

    public async ValueTask SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry)
    {
        var findCache = await Db.SqliteCache
            .Where(x => x.CacheKey == cacheKey)
            .FirstOrDefaultAsync();

        if (findCache == null)
        {
            _log.Verbose("Set CacheTower sqlite layer. Key: {CacheKey}", cacheKey);
            await Db.SqliteCache.AddAsync(new SqliteCacheEntity() {
                CacheKey = cacheKey,
                ExpiryDate = cacheEntry.Expiry,
                CreatedDate = DateTime.UtcNow,
                Value = cacheEntry.Value.ToJson(),
            });
        }
        else
        {
            _log.Verbose("Update CacheTower sqlite layer. Key: {CacheKey}", cacheKey);
            findCache.ExpiryDate = cacheEntry.Expiry;
            findCache.Value = cacheEntry.Value.ToJson();
        }

        await Db.SaveChangesAsync();
        _log.Verbose("Save CacheTower sqlite layer done");
    }

    public async ValueTask<bool> IsAvailableAsync(string cacheKey)
    {
        var obj = await Db.SqliteCache.AsNoTracking()
            .Where(x => x.CacheKey == cacheKey)
            .FirstOrDefaultAsync();

        var isAvailable = obj != null;
        _log.Verbose("CacheTower SQLite layer is available: {IsAvailable}", isAvailable);

        return isAvailable;
    }
}
