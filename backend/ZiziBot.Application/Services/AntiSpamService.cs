using System.Net;
using Flurl;
using Flurl.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Common.Dtos;
using ZiziBot.Application.Common.Types;

namespace ZiziBot.Application.Services;

public class AntiSpamService(
    ILogger<AntiSpamService> logger,
    ICacheService cacheService,
    DataFacade dataFacade
)
{
    private const string DEFAULT_STALE_TIME = "10m";

    public async Task<AntiSpamDto> CheckSpamAsync(long chatId, long userId)
    {
        var antispamDto = new AntiSpamDto();

        if (userId == 0 ||
            chatId == 0)
            return antispamDto;

        var taskCheckEss = CheckEssAsync(userId);
        var taskCheckCombotAntiSpam = CheckCombotAntiSpamAsync(userId);
        var taskCheckSpamWatch = CheckSpamWatchAntiSpamAsync(userId);
        await Task.WhenAll(taskCheckEss, taskCheckCombotAntiSpam, taskCheckSpamWatch);

        antispamDto.IsBanEss = (await taskCheckEss).IsBanEss;
        antispamDto.IsBanCasFed = (await taskCheckCombotAntiSpam).IsBanCasFed;
        antispamDto.IsBanSwFed = (await taskCheckSpamWatch).IsBanSwFed;

        antispamDto.CasRecord = (await taskCheckCombotAntiSpam).CasRecord;
        antispamDto.SpamWatchRecord = (await taskCheckSpamWatch).SpamWatchRecord;

        return antispamDto;
    }

    private async Task<AntiSpamDto> CheckEssAsync(long userId)
    {
        var cacheData = await cacheService.GetOrSetAsync(new CacheParam<AntiSpamDto>() {
            CacheKey = CacheKey.USER_BAN_ESS + userId,
            StaleAfter = DEFAULT_STALE_TIME,
            Action = async () => {
                var antiSpamDto = new AntiSpamDto();
                var globalBanEntities = await dataFacade.MongoDb.GlobalBan
                    .Where(entity => entity.UserId == userId && entity.Status == EventStatus.Complete)
                    .ToListAsync();

                antiSpamDto.IsBanEss = globalBanEntities.Count != 0;

                return antiSpamDto;
            }
        });

        return cacheData;
    }

    private async Task<AntiSpamDto> CheckCombotAntiSpamAsync(long userId)
    {
        var cacheData = await cacheService.GetOrSetAsync(new CacheParam<AntiSpamDto>() {
            CacheKey = CacheKey.USER_BAN_CAS + userId,
            StaleAfter = DEFAULT_STALE_TIME,
            Action = async () => {
                var antiSpamDto = new AntiSpamDto();

                try
                {
                    var url = UrlConst.ANTISPAM_COMBOT_API.SetQueryParam("userId", userId);
                    var antispamApiDto = await url.AllowAnyHttpStatus().GetJsonAsync<CombotAntispamApiDto>();

                    logger.LogDebug("Combot AntiSpam for {UserId}: {@Dto}", userId, antispamApiDto);
                    antiSpamDto.IsBanCasFed = antispamApiDto.IsBanned;
                    antiSpamDto.CasRecord = antispamApiDto.Result;
                }
                catch (Exception exception)
                {
                    logger.LogWarning(exception, "Fail to check Combot AntiSpam for {UserId}", userId);
                }

                return antiSpamDto;
            }
        });

        return cacheData;
    }

    private async Task<AntiSpamDto> CheckSpamWatchAntiSpamAsync(long userId)
    {
        var cacheData = await cacheService.GetOrSetAsync(new CacheParam<AntiSpamDto>() {
            CacheKey = CacheKey.USER_BAN_SW + userId,
            StaleAfter = DEFAULT_STALE_TIME,
            Action = async () => {
                var antiSpamDto = new AntiSpamDto();

                try
                {
                    var spamwatchResult = new SpamWatchResult();
                    var url = UrlConst.ANTISPAM_SPAMWATCH_API.AppendPathSegment(userId);
                    var apiKey = await dataFacade.Bot.GetApiKeyAsync(ApiKeyCategory.Internal, ApiKeyVendor.SpamWatch);

                    if (apiKey.IsNotNullOrEmpty())
                        return antiSpamDto;

                    var spamwatchDto = await url.AllowAnyHttpStatus()
                        .WithOAuthBearerToken(apiKey)
                        .GetJsonAsync<SpamwatchDto>();

                    spamwatchResult.IsBanned = spamwatchDto.Code == (int)HttpStatusCode.OK;
                    spamwatchResult.BanRecord = spamwatchDto;
                    antiSpamDto.SpamWatchRecord = spamwatchDto;

                    logger.LogDebug("Spamwatch AntiSpam for {UserId}: {@Dto}", userId, spamwatchDto);
                }
                catch (Exception exception)
                {
                    logger.LogWarning(exception, "Fail to check Spamwatch AntiSpam for {UserId}", userId);
                }

                return antiSpamDto;
            }
        });

        return cacheData;
    }
}