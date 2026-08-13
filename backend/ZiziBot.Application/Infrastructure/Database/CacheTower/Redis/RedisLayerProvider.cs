using CacheTower;
using Serilog;
using StackExchange.Redis;

namespace ZiziBot.Application.Infrastructure.Database.CacheTower.Redis;

internal class RedisLayerProvider(
    string connectionString,
    RedisLayerOptions options
)
    : IDistributedCacheLayer
{
    private string ConnectionString { get; set; } = connectionString;
    private ConnectionMultiplexer? Connection { get; set; }
    private IDatabaseAsync Database => Connection?.GetDatabase(options.DatabaseIndex) ?? throw new NullReferenceException("Redis database not set because connection fail");
    private static string PrefixRoot => ValueConst.UniqueKey;
    private readonly ILogger _log = Log.ForContext<RedisLayerProvider>();
    private bool IsAvailable { get; set; }

    public ValueTask CleanupAsync()
    {
        //Noop as Redis handles this directly
        return new ValueTask();
    }

    public async ValueTask EvictAsync(string cacheKey)
    {
        await TryConnect();

        _log.Verbose("Preparing evict CacheTower redis layer. Key: {CacheKey}", cacheKey);
        await Database.KeyDeleteAsync(CacheKey(cacheKey));
        _log.Verbose("Evict CacheTower redis layer. Key: {CacheKey}", cacheKey);
    }

    public async ValueTask FlushAsync()
    {
        await TryConnect();

        _log.Verbose("Preparing flush CacheTower redis layer");
        var redisEndpoints = Connection?.GetEndPoints();

        if (redisEndpoints == null ||
            Connection == null)
        {
            return;
        }

        foreach (var endpoint in redisEndpoints)
        {
            var keys = Connection.GetServer(endpoint).KeysAsync(options.DatabaseIndex, pattern: $"{PrefixRoot}*");

            await foreach (var redisKey in keys)
            {
                await Database.KeyDeleteAsync(redisKey);
            }
        }

        _log.Verbose("Flush CacheTower redis layer has done");
    }

    public async ValueTask<CacheEntry<T>?> GetAsync<T>(string cacheKey)
    {
        await TryConnect();

        _log.Verbose("Preparing get CacheTower redis layer. Key: {CacheKey}", cacheKey);

        var redisValue = await Database.StringGetAsync(CacheKey(cacheKey));

        if (redisValue == RedisValue.Null ||
            redisValue == RedisValue.EmptyString)
            return null;

        using var stream = new MemoryStream(redisValue);
        _log.Verbose("Get CacheTower redis layer. Key: {CacheKey}", cacheKey);
        return options.Serializer.Deserialize<CacheEntry<T>>(stream);
    }

    public async ValueTask<bool> IsAvailableAsync(string cacheKey)
    {
        await TryConnect();

        return IsAvailable;
    }

    public async ValueTask SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry)
    {
        await TryConnect();

        var expiryOffset = cacheEntry.Expiry - DateTime.UtcNow;

        if (expiryOffset < TimeSpan.Zero)
        {
            return;
        }

        _log.Verbose("Preparing set CacheTower redis layer. Key: {CacheKey}", cacheKey);

        using var stream = new MemoryStream();
        options.Serializer.Serialize(stream, cacheEntry);
        stream.Seek(0, SeekOrigin.Begin);
        var redisValue = RedisValue.CreateFrom(stream);
        var result = await Database.StringSetAsync(CacheKey(cacheKey), redisValue, expiryOffset);
        _log.Verbose("Set CacheTower redis layer. Key: {CacheKey}, Expiry: {Expiry}, Result: {Result}", cacheKey, expiryOffset, result);
    }

    private async Task TryConnect()
    {
        try
        {
            _log.Verbose("Connecting to CacheTower redis layer");

            if (Connection?.IsConnected == true)
            {
                IsAvailable = true;
                _log.Verbose("Connected to CacheTower redis layer, previous connection, {ClientName}", Connection.ClientName);
                return;
            }

            Connection = await ConnectionMultiplexer.ConnectAsync(ConnectionString);
            IsAvailable = true;

            _log.Verbose("Connected to CacheTower redis layer");
        }
        catch (Exception e)
        {
            _log.Error("CacheTower redis layer failed connecting to redis. Message: {Message}", e.Message);
            IsAvailable = false;
        }
    }

    private string CacheKey(string cacheKey)
    {
        var newCacheKey = PrefixRoot.IsNullOrEmpty() ? $"{cacheKey}".Replace("/", ":") : $"{PrefixRoot}/{cacheKey}".Replace("/", ":");
        _log.Verbose("Convert CacheTower redis layer. Key: {CacheKey} => {NewCacheKey}", cacheKey, newCacheKey);
        return newCacheKey;
    }
}
