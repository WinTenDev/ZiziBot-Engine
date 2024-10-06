using Flurl.Http;
using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Services;

public class UupDumpService(
    ILogger<UupDumpService> logger,
    CacheService cacheService)
{
    private const string ListUpdatesApi = "https://uupdump.net/json-api/listid.php";

    public async Task<BuildUpdate> GetUpdatesAsync(string? search = null)
    {
        var buildUpdate = await cacheService.GetOrSetAsync(
            cacheKey: $"vendor/{ListUpdatesApi.ForCacheKey()}",
            action: async () => {
                var obj = await ListUpdatesApi.GetJsonAsync<BuildUpdate>();

                return obj;
            }
        );

        var filteredBuilds = buildUpdate.Response.Builds
            .WhereIf(search != null, build => build.BuildNumber.Contains(search))
            .ToList();

        logger.LogDebug("Found about UUP: {FilteredBuilds} of {AllBuilds} build(s)", filteredBuilds.Count, buildUpdate.Response.Builds.Count);

        var filteredUpdate = new BuildUpdate
        {
            JsonApiVersion = buildUpdate.JsonApiVersion,
            Response = new Response
            {
                ApiVersion = buildUpdate.Response.ApiVersion,
                Builds = filteredBuilds
            }
        };

        return filteredUpdate;
    }
}