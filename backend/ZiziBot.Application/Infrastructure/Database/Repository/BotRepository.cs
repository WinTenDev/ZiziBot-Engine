using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Infrastructure.Database.Service;
using ZiziBot.Application.Common.Dtos.Entity;
using ZiziBot.Application.Common.Types;
using ZiziBot.Application.Infrastructure.Database.MongoDb;

namespace ZiziBot.Application.Infrastructure.Database.Repository;

public class BotRepository(
    ILogger<BotRepository> logger,
    MongoDbContext mongoDbContext,
    CacheService cacheService
)
{
    public async Task<BotSettingDto> GetBotMain()
    {
        var cached = await cacheService.GetOrSetAsync(new CacheParam<BotSettingDto>()
        {
            CacheKey = CacheKey.GLOBAL_BOT_MAIN,
            Action = async () =>
            {
                var botMain = await mongoDbContext.BotSettings.AsNoTracking()
                    .Where(entity => entity.Name == "Main")
                    .Where(entity => entity.Status == EventStatus.Complete)
                    .Select(x => new BotSettingDto
                    {
                        Name = x.Name,
                        Token = x.Token
                    })
                    .FirstOrDefaultAsync();

                botMain.EnsureNotNull();

                return botMain;
            }
        });


        return cached;
    }


    public async Task<List<BotSettingDto>> ListBots()
    {
        var listBotData = await mongoDbContext.BotSettings
            .Where(settings => settings.Status == EventStatus.Complete)
            .Select(x => new BotSettingDto
            {
                Name = x.Name,
                Token = x.Token
            })
            .ToListAsync();

        return listBotData;
    }

    public async Task<string> GetRequiredApiKeyAsync(ApiKeyCategory category, ApiKeyVendor name)
    {
        var apiKey = await GetApiKeyAsync(category, name);

        return apiKey ?? throw new InvalidOperationException("Api key not found");
    }

    public async Task<string> GetApiKeyAsync(ApiKeyCategory category, ApiKeyVendor name)
    {
        var cache = await cacheService.GetOrSetAsync($"api-key/{category}/{name}", async () =>
        {
            var query = mongoDbContext.ApiKey.AsNoTracking();

            if (name == ApiKeyVendor.GitHub)
                query = query.OrderByDescending(x => x.Remaining);
            else
                query = query.OrderBy(entity => entity.LastUsedDate);

            query = query.Where(entity => entity.Status == EventStatus.Complete)
                .Where(entity => entity.Category == category)
                .Where(entity => entity.Name == name);

            var apiKey = await query.FirstOrDefaultAsync();

            return apiKey?.ApiKey ?? string.Empty;
        });

        return cache;
    }
}
