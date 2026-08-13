using CacheTower;
using Serilog;

namespace ZiziBot.Application.Infrastructure.Database.CacheTower.Json;

public class JsonLayerProvider : ILocalCacheLayer
{
    public required string DirPath { get; set; }
    public required ICacheSerializer Serializer { get; set; }
    private readonly ILogger _log = Log.ForContext<JsonLayerProvider>();

    public async ValueTask FlushAsync()
    {
        _log.Verbose("Flush CacheTower JSON layer");
        await CleanupCache(true);
        _log.Verbose("Flush CacheTower JSON layer has done");
    }

    public async ValueTask CleanupAsync()
    {
        _log.Verbose("Cleanup CacheTower JSON layer");
        await CleanupCache();
        _log.Verbose("Cleanup CacheTower JSON layer has done");
    }

    public async ValueTask EvictAsync(string cacheKey)
    {
        _log.Verbose("Evict CacheTower JSON layer. Key: {CacheKey}", cacheKey);
        var jsonFile = CacheToFile(cacheKey: cacheKey);
        await jsonFile.DeleteThreadSafeAsync();
        _log.Verbose("Evict CacheTower JSON layer has done. Key: {CacheKey}", cacheKey);
    }

    public async ValueTask<CacheEntry<T>?> GetAsync<T>(string cacheKey)
    {
        _log.Verbose("Get CacheTower JSON layer. Key: {CacheKey}", cacheKey);
        var jsonFile = CacheToFile(cacheKey: cacheKey);

        if (!File.Exists(path: jsonFile))
            return null;

        var json = await jsonFile.ReadAllTextThreadSafeAsync();

        var content = json.ToObject<JsonCacheEntry<T>>();

        var cacheEntry = new CacheEntry<T>(Value: content.CacheValue, Expiry: content.ExpireDate);
        _log.Verbose("Get CacheTower JSON layer. Key: {CacheKey}, Expiry: {Expiry}", cacheKey, cacheEntry.Expiry);

        return cacheEntry;
    }

    public async ValueTask SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry)
    {
        _log.Verbose("Set CacheTower JSON layer. Key: {CacheKey}", cacheKey);
        var jsonFile = CacheToFile(cacheKey: cacheKey);
        var content = new JsonCacheEntry<T> {
            CacheKey = cacheKey,
            CreatedDate = DateTime.UtcNow,
            ExpireDate = cacheEntry.Expiry,
            CacheValue = cacheEntry.Value
        };

        await jsonFile.WriteAllTextThreadSafeAsync(content.ToJson(true));
        _log.Verbose("Set CacheTower JSON layer. Key: {CacheKey}", cacheKey);
    }

    public async ValueTask<bool> IsAvailableAsync(string cacheKey)
    {
        await Task.Delay(1);

        var isAvailable = DirPath.EnsureDirectory().IsNotNullOrWhiteSpace();
        _log.Verbose("CacheTower JSON layer is available: {IsAvailable}", isAvailable);

        return isAvailable;
    }

    private async Task CleanupCache(bool any = false)
    {
        await Task.Delay(1);

        var data = DirPath.GetFiles().Select(x => new {
            Path = x.FullName,
            CacheEntry = File.ReadAllText(x.FullName).ToObject<JsonCacheEntry<object>>()
        }).ToList();

        foreach (var cache in data)
        {
            var cacheEntry = cache.CacheEntry;
            _log.Debug("Checking JSON cache: {Path}, ExpireDate: {ExpireDate}", cache.Path, cacheEntry.ExpireDate);
            if (cacheEntry.ExpireDate > DateTime.UtcNow && !any)
                continue;

            _log.Debug("Deleting JSON cache: {Path}", cache.Path);
            await cache.Path.DeleteThreadSafeAsync();
            _log.Debug("Deleted JSON cache: {Path}", cache.Path);
        }

        DirPath.DeleteEmptyDirectories();
    }

    private string CacheToFile(string cacheKey)
    {
        var fileName = cacheKey.Replace("/", $"{Path.DirectorySeparatorChar}") + ".json";
        var jsonPath = Path.Combine(DirPath, fileName);

        return jsonPath.EnsureDirectory();
    }
}
