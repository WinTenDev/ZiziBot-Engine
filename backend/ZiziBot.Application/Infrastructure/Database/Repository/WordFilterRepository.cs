using Microsoft.EntityFrameworkCore;
using ZiziBot.Application.Core.Interfaces;
using ZiziBot.Application.Infrastructure.Database.MongoDb;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Infrastructure.Database.Repository;

public class WordFilterRepository(
    MongoDbContext mongoDbContext,
    ICacheService cacheService
)
{
    public async Task SaveAsync(WordFilterDto dto)
    {
        var wordFilter = await mongoDbContext.WordFilter
            .Where(x => x.Status == EventStatus.Complete)
            .Where(x => x.Word == dto.Word)
            .FirstOrDefaultAsync();

        if (wordFilter == null)
        {
            mongoDbContext.WordFilter.Add(new WordFilterEntity() {
                ChatId = dto.ChatId,
                UserId = dto.UserId,
                Word = dto.Word,
                IsGlobal = dto.IsGlobal,
                IsRegex = dto.IsRegex,
                Action = dto.Action,
                Status = EventStatus.Complete,
                TransactionId = dto.TransactionId
            });
        }
        else
        {
            wordFilter.ChatId = dto.ChatId;
            wordFilter.UserId = dto.UserId;
            wordFilter.Action = dto.Action;
            wordFilter.IsRegex = dto.IsRegex;
            wordFilter.TransactionId = dto.TransactionId;
        }

        await mongoDbContext.SaveChangesAsync();
        await GetAllAsync(true);
    }

    public async Task<List<WordFilterDto>> GetAllAsync(bool evictAfter = false)
    {
        var cache = await cacheService.GetOrSetAsync(
            cacheKey: CacheKey.CHAT_BADWORD,
            evictAfter: evictAfter,
            action: async () =>
            {
                var data = await mongoDbContext.WordFilter
                    .Where(x => x.Status == EventStatus.Complete)
                    .Select(entity => new WordFilterDto() {
                        Id = entity.Id.ToString(),
                        ChatId = entity.ChatId,
                        UserId = entity.UserId,
                        Word = entity.Word,
                        Action = entity.Action,
                        IsGlobal = entity.IsGlobal,
                        IsRegex = entity.IsRegex == true,
                        CreatedDate = entity.CreatedDate,
                        UpdatedDate = entity.UpdatedDate
                    })
                    .OrderByDescending(x => x.CreatedDate)
                    .ToListAsync();

                return data;
            });

        return cache;
    }
}
