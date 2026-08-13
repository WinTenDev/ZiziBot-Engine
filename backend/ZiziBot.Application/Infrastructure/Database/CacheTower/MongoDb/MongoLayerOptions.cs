using CacheTower;

namespace ZiziBot.Application.Infrastructure.Database.CacheTower.MongoDb;

internal record struct MongoLayerOptions(
    string ConnectionString,
    ICacheSerializer Serializer
);
