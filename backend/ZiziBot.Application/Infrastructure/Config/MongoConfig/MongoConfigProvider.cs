using Microsoft.Extensions.Configuration;

namespace ZiziBot.Application.Infrastructure.Config.MongoConfig;

public class MongoConfigProvider(MongoConfigSource mongoConfigSource) : ConfigurationProvider
{
    public override void Load()
    {
        Data = mongoConfigSource.GetAppSettings();
    }
}