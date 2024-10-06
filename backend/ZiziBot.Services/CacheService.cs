using CacheTower;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ZiziBot.Services;

public class CacheService(
    ILogger<CacheService> logger,
    IOptions<CacheConfig> cacheConfig,
    ICacheStack cacheStack)
    : ICacheService
{
    private readonly CacheConfig _cacheConfig = cacheConfig.Value;
    private string _expireAfter = "24h";
    private string _staleAfter = "15s";

    public async Task<T> GetOrSetAsync<T>(
        string cacheKey,
        Func<Task<T>> action,
        bool disableCache = false,
        bool evictBefore = false,
        bool evictAfter = false,
        string? expireAfter = null,
        string? staleAfter = null
    )
    {
        if (disableCache)
            return await action();

        if (evictBefore)
            await EvictAsync(cacheKey);

        if (expireAfter != null) _expireAfter = expireAfter;
        if (staleAfter != null) _staleAfter = staleAfter;

        var expireAfterSpan = _expireAfter.ToTimeSpan();
        var staleAfterSpan = _staleAfter.ToTimeSpan();

        try
        {
            logger.LogDebug("Loading Cache with Key: {CacheKey}. StaleAfter: {StaleAfter}. ExpireAfter: {ExpireAfter}",
                cacheKey, staleAfterSpan, expireAfterSpan);

            var cacheSettings = new CacheSettings(expireAfterSpan, staleAfterSpan);

            var cache = await cacheStack.GetOrSetAsync<T>(
                cacheKey: cacheKey.Trim(),
                valueFactory: async (_) => {
                    logger.LogDebug(
                        "Updating cache with Key: {CacheKey}. StaleAfter: {StaleAfter}. ExpireAfter: {ExpireAfter}",
                        cacheKey, staleAfterSpan, expireAfterSpan);

                    return await action();
                },
                settings: cacheSettings
            );

            if (evictAfter)
                await EvictAsync(cacheKey);

            return cache;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error loading cache with Key: {Key}", cacheKey);

            await EvictAsync(cacheKey);

            var data = await action();

            return data;
        }
    }

    public async Task EvictAsync(string cacheKey)
    {
        try
        {
            logger.LogDebug("Evicting cache with key: {CacheKey}", cacheKey);
            await cacheStack.EvictAsync(cacheKey);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Fail to evict cache Key: {Key}", cacheKey);

            if (_cacheConfig.UseJsonFile)
                Directory.Delete(PathConst.CACHE_TOWER_PATH, true);
        }
    }
}