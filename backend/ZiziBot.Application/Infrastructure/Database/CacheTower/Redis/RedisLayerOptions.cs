using CacheTower;

namespace ZiziBot.Application.Infrastructure.Database.CacheTower.Redis;

internal record struct RedisLayerOptions(
    ICacheSerializer Serializer,
    int DatabaseIndex = -1,
    string PrefixRoot = ""
);
