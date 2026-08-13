using Microsoft.Extensions.DependencyInjection;
using NeoSmart.Caching.Sqlite;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.NewtonsoftJson;

namespace ZiziBot.Application.Infrastructure.Database.Extensions;

public static class FusionCacheExtension
{
    public static void ConfigureFusionCache(this IServiceCollection services)
    {
        // services.AddSqliteCache(options =>
        // {
        //     options.CachePath = PathConst.FUSION_CACHE_SQLITE.EnsureDirectory();
        // }, new SQLite3Provider_e_sqlite3());

        services.AddFusionCache()
            .WithSerializer(new FusionCacheNewtonsoftJsonSerializer())
            .WithDefaultEntryOptions(options => { options.Duration = TimeSpan.FromMinutes(10); })
            .WithDistributedCache(x => new SqliteCache(new SqliteCacheOptions()
            {
                CachePath = PathConst.FUSION_CACHE_SQLITE.EnsureDirectory()
            }))
            // .WithRegisteredDistributedCache()
            ;
    }
}
