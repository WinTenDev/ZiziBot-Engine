using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Common.Types;
using ZiziBot.Application.Infrastructure.Vendor.FathimahApi.v2;

namespace ZiziBot.Application.Services;

public sealed class FathimahRestService(
    ILogger<FathimahRestService> logger,
    CacheService cacheService
)
{
    private const int MaxRetryAttempts = 3;

    public async Task<CityResponse> GetAllCityAsync()
    {
        const string path = "sholat/kota/semua";

        logger.LogInformation("Get City");

        var apis = await cacheService.GetOrSetAsync(new CacheParam<CityResponse>()
        {
            CacheKey = $"vendor/bang-hasan/{path}",
            ExpireAfter = "1d",
            StaleAfter = "1h",
            EvictAfter = true,
            Action = async () =>
            {
                return await GetJsonWithRetryAsync<CityResponse>(
                    UrlConst.FATHIMAH_API.AppendPathSegment(path)
                );
            }
        });

        return apis;
    }

    public async Task<KeyValuePair<string, string>?> GetCurrentShalatTime(long cityId)
    {
        var currentTime = DateTime.UtcNow.AddHours(Env.DEFAULT_TIMEZONE).ToString("HH:mm");
        var shalatTime = await GetShalatTime(DateTime.Now, cityId);
        var currentShalat = shalatTime.Schedule?.ShalatDict.FirstOrDefault(pair => pair.Value == currentTime);

        return currentShalat;
    }

    public async Task<ShalatTimeResponse> GetShalatTime(long cityId, bool evictBefore = false)
    {
        return await GetShalatTime(DateTime.UtcNow.AddHours(Env.DEFAULT_TIMEZONE), cityId, evictBefore);
    }

    public async Task<ShalatTimeResponse> GetShalatTime(DateTime dateTime, long cityId, bool evictBefore = false)
    {
        var date = dateTime.ToString("yyyy-MM-dd");
        var path = $"sholat/jadwal/{cityId}/{date}";

        logger.LogInformation("Get Shalat time for ChatId: {CityId} with Date: {DateStr}", cityId, dateTime);

        var apis = await cacheService.GetOrSetAsync(new CacheParam<ShalatTimeResponse>()
            {
                CacheKey = $"vendor/bang-hasan/{path}",
                ExpireAfter = "1d",
                EvictBefore = evictBefore,
                StaleAfter = "1h",
                Action = async () =>
                {
                    return await GetJsonWithRetryAsync<ShalatTimeResponse>(
                        UrlConst.FATHIMAH_API.AppendPathSegment(path)
                    );
                }
            }
        );

        logger.LogInformation("Shalat time for ChatId: {CityId} with Date: {DateStr} is: {ShalatTime}", cityId, dateTime, apis.Schedule?.Daerah);

        return apis;
    }

    private async Task<T> GetJsonWithRetryAsync<T>(Url url, int maxAttempts = MaxRetryAttempts)
    {
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await url.GetJsonAsync<T>();
            }
            catch (FlurlHttpException exception) when (exception.StatusCode == 429 && attempt < maxAttempts)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                logger.LogWarning(
                    exception,
                    "Fathimah API rate limited. Retrying {Attempt}/{MaxAttempts} after {DelaySeconds}s. Url: {Url}",
                    attempt,
                    maxAttempts,
                    delay.TotalSeconds,
                    url
                );
                await Task.Delay(delay);
            }
        }

        return await url.GetJsonAsync<T>();
    }
}
