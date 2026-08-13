using Flurl.Http;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Infrastructure.Vendor.UupDump;

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
                var json = await ListUpdatesApi.GetStringAsync();
                var obj = json.ToObject<BuildUpdate>();

                return obj;
            }
        );

        var filteredBuilds = buildUpdate?.Response.Builds
            .WhereIf(search != null, build => build.BuildNumber.Contains(search))
            .ToList();

        logger.LogDebug("Found about UUP: {FilteredBuilds} of {AllBuilds} build(s)", filteredBuilds?.Count, buildUpdate?.Response.Builds.Count);

        var filteredUpdate = new BuildUpdate
        {
            JsonApiVersion = buildUpdate?.JsonApiVersion ?? string.Empty,
            Response = new Response
            {
                ApiVersion = buildUpdate?.Response?.ApiVersion ?? string.Empty,
                Builds = filteredBuilds ?? new List<Build>()
            }
        };

        return filteredUpdate;
    }
}