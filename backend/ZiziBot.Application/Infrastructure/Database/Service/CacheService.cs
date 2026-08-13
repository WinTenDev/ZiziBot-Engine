using System;
using CacheTower;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;
using ZiziBot.Application.Core.Interfaces;
using ZiziBot.Application.Common.Types;

namespace ZiziBot.Application.Infrastructure.Database.Service;

public class CacheService(
    ILogger<CacheService> logger,
    IOptions<CacheConfig> cacheConfig,
    ICacheStack cacheStack,
    IFusionCache fusionCache
)
    : ICacheService
{
    private readonly CacheConfig _cacheConfig = cacheConfig.Value;
    private const string DefaultExpireAfter = "24h";
    private const string DefaultStaleAfter = "15s";

    public async Task<T> GetOrSetAsync<T>(
        string cacheKey,
        Func<Task<T>> action,
        bool disableCache = false,
        bool evictBefore = false,
        bool evictAfter = false,
        string? expireAfter = null,
        string? staleAfter = null,
        bool throwIfError = false
    )
    {
        if (disableCache)
        {
            logger.LogDebug(
                "Cache bypassed. Key={CacheKey} Type={CacheType} DisableCache={DisableCache}",
                cacheKey,
                typeof(T).Name,
                disableCache
            );
            return await action();
        }

        if (evictBefore)
        {
            logger.LogDebug(
                "Cache evict-before requested. Key={CacheKey} Type={CacheType}",
                cacheKey,
                typeof(T).Name
            );
            await EvictAsync(cacheKey);
        }

        var resolvedExpireAfter = expireAfter ?? DefaultExpireAfter;
        var resolvedStaleAfter = staleAfter ?? DefaultStaleAfter;

        var expireAfterSpan = resolvedExpireAfter.ToTimeSpan();
        var staleAfterSpan = resolvedStaleAfter.ToTimeSpan();

        cacheKey = cacheKey.ForCacheKey();

        try
        {
            var useFusionCache = _cacheConfig.CacheEngine != CacheEngine.CacheTower;

            logger.LogDebug(
                "Cache resolve started. Key={CacheKey} Type={CacheType} Engine={CacheEngine} StaleAfter={StaleAfter} ExpireAfter={ExpireAfter} EvictBefore={EvictBefore} EvictAfter={EvictAfter}",
                cacheKey,
                typeof(T).Name,
                useFusionCache ? "FusionCache" : "CacheTower",
                staleAfterSpan,
                expireAfterSpan,
                evictBefore,
                evictAfter
            );

            var cache = useFusionCache
                ? await GetOrSetWithFusionCacheAsync(cacheKey, action, expireAfterSpan, staleAfterSpan)
                : await GetOrSetWithCacheTowerAsync(cacheKey, action, expireAfterSpan, staleAfterSpan);

            logger.LogDebug(
                "Cache resolve completed. Key={CacheKey} Type={CacheType} StaleAfter={StaleAfter} ExpireAfter={ExpireAfter}",
                cacheKey,
                typeof(T).Name,
                staleAfterSpan,
                expireAfterSpan
            );

            if (evictAfter)
            {
                logger.LogDebug(
                    "Cache evict-after requested. Key={CacheKey} Type={CacheType}",
                    cacheKey,
                    typeof(T).Name
                );
                await EvictAsync(cacheKey);
            }

            return cache;
        }
        catch (ObjectDisposedException disposedException)
        {
            logger.LogWarning(
                disposedException, "Cache instance was disposed during operation. Falling back to direct action. Key={CacheKey} Type={CacheType}",
                cacheKey,
                typeof(T).Name
            );

            var data = await action();
            return data;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Cache resolve failed. Key={CacheKey} Type={CacheType} ThrowIfError={ThrowIfError}",
                cacheKey,
                typeof(T).Name,
                throwIfError
            );

            if (throwIfError)
                throw;

            logger.LogWarning(
                exception, "Falling back to direct action after cache failure. Key={CacheKey} Type={CacheType}",
                cacheKey,
                typeof(T).Name
            );

            await EvictAsync(cacheKey);

            var data = await action();

            return data;
        }
    }

    public Task<T> GetOrSetAsync<T>(CacheParam<T> cacheParam)
    {
        return GetOrSetAsync(cacheParam.CacheKey,
            cacheParam.Action,
            cacheParam.DisableCache,
            cacheParam.EvictBefore,
            cacheParam.EvictAfter,
            cacheParam.ExpireAfter,
            cacheParam.StaleAfter,
            cacheParam.ThrowIfError
        );
    }



    public async Task EvictAsync(string cacheKey)
    {
        try
        {
            logger.LogDebug("Cache eviction started. Key={CacheKey}", cacheKey);
            await cacheStack.EvictAsync(cacheKey);
            await fusionCache.RemoveAsync(cacheKey);
            logger.LogDebug("Cache eviction completed. Key={CacheKey}", cacheKey);
        }
        catch (ObjectDisposedException disposedException)
        {
            logger.LogWarning(
                disposedException, "Cache instance was disposed during eviction. Skipping eviction. Key={CacheKey}", cacheKey);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Cache eviction failed. Key={CacheKey}", cacheKey);

            if (_cacheConfig.UseJsonFile)
            {
                logger.LogWarning(
                    exception, "Deleting CacheTower JSON directory after eviction failure. Path={CachePath}",
                    PathConst.CACHE_TOWER_JSON
                );
                PathConst.CACHE_TOWER_JSON.DeleteDirectory();
            }
        }
    }



    private async Task<T> GetOrSetWithFusionCacheAsync<T>(
        string cacheKey,
        Func<Task<T>> action,
        TimeSpan expireAfterSpan,
        TimeSpan staleAfterSpan
    )
    {
        return await fusionCache.GetOrSetAsync<T>(
            key: cacheKey.Trim(),
            factory: async (ctx, ct) =>
            {
                logger.LogDebug(
                    "FusionCache value factory invoked. Key={CacheKey} Type={CacheType} StaleAfter={StaleAfter} ExpireAfter={ExpireAfter}",
                    cacheKey,
                    typeof(T).Name,
                    staleAfterSpan,
                    expireAfterSpan
                );

                return await action();
            },
            options: new FusionCacheEntryOptions
            {
                Duration = expireAfterSpan
            }
        );
    }

    private async Task<T> GetOrSetWithCacheTowerAsync<T>(
        string cacheKey,
        Func<Task<T>> action,
        TimeSpan expireAfterSpan,
        TimeSpan staleAfterSpan
    )
    {
        var cacheSettings = new CacheSettings(expireAfterSpan, staleAfterSpan);

        return await cacheStack.GetOrSetAsync<T>(
            cacheKey: cacheKey.Trim(),
            valueFactory: async (_) =>
            {
                logger.LogDebug(
                    "Cache Tower value factory invoked. Key={CacheKey} Type={CacheType} StaleAfter={StaleAfter} ExpireAfter={ExpireAfter}",
                    cacheKey,
                    typeof(T).Name,
                    staleAfterSpan,
                    expireAfterSpan
                );

                return await action();
            },
            settings: cacheSettings
        );
    }
}
