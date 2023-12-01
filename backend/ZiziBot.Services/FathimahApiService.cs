using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using ZiziBot.Types.Vendor.FathimahApi;

namespace ZiziBot.Services;

public class FathimahApiService
{
    private const string BaseUrl = "https://api.banghasan.com";

    private readonly ILogger<FathimahApiService> _logger;
    private readonly CacheService _cacheService;

    public FathimahApiService(ILogger<FathimahApiService> logger, CacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<CityResponse> GetAllCityAsync()
    {
        const string path = "sholat/format/json/kota";

        _logger.LogInformation("Get City");

        var apis = await _cacheService.GetOrSetAsync(
            cacheKey: $"vendor/api-banghasan-com/shalat/list-kota",
            expireAfter: "1d",
            staleAfter: "1h",
            action: async () => {
                var apis = await BaseUrl.AppendPathSegment(path).GetJsonAsync<CityResponse>();

                return apis;
            }
        );

        return apis;
    }

    public async Task<ShalatTimeResponse> GetShalatTime(long cityId)
    {
        return await GetShalatTime(DateTime.UtcNow.AddHours(Env.DEFAULT_TIMEZONE), cityId);
    }

    public async Task<ShalatTimeResponse> GetShalatTime(DateTime dateTime, long cityId)
    {
        var dateStr = dateTime.ToString("yyyy-MM-dd");
        var path = $"sholat/format/json/jadwal/kota/{cityId}/tanggal/{dateStr}";

        _logger.LogInformation("Get Shalat time for ChatId: {CityId} with Date: {DateStr}", cityId, dateStr);

        var apis = await _cacheService.GetOrSetAsync(
            cacheKey: $"vendor/api-banghasan-com/shalat/kota/{cityId}",
            expireAfter: "1d",
            staleAfter: "1h",
            action: async () => {
                var apis = await BaseUrl
                    .AppendPathSegment(path)
                    .GetJsonAsync<ShalatTimeResponse>();

                return apis;
            }
        );

        return apis;
    }
}