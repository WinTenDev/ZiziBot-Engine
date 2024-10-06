using Microsoft.Extensions.Configuration;

namespace ZiziBot.Infrastructure.MongoConfig;

public class MongoConfigProvider(MongoConfigSource mongoConfigSource) : ConfigurationProvider
{
    public override void Load()
    {
        Data = mongoConfigSource.GetAppSettings();
    }
}