using Microsoft.EntityFrameworkCore;
using ZiziBot.Application.Infrastructure.Database.Utils;
using ZiziBot.Application.Core.Interfaces;
using ZiziBot.Application.Infrastructure.Database.MongoDb;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Infrastructure.Database.Repository;

public class GroupRepository(MongoDbContext mongoDbContext, ICacheService cacheService)
{
    public async Task<WelcomeMessageDto?> GetWelcomeMessageById(string welcomeId)
    {
        var query = await mongoDbContext.WelcomeMessage
            .AsNoTracking()
            .Where(entity => entity.Id == welcomeId.ToObjectId())
            .FirstOrDefaultAsync();

        if (query == null)
            return null;

        var listChatSetting = await mongoDbContext.ChatSetting
            .Where(entity => entity.ChatId == query.ChatId)
            .FirstOrDefaultAsync();

        if (listChatSetting == null)
            return null;

        var data = new WelcomeMessageDto() {
            Id = query.Id.ToString(),
            ChatId = query.ChatId,
            ChatTitle = listChatSetting.ChatTitle,
            Text = query.Text,
            RawButton = query.RawButton,
            Media = query.Media,
            DataType = (int)query.DataType,
            DataTypeName = query.DataType.ToString(),
            Status = (int)query.Status,
            StatusName = ((EventStatus)query.Status).ToString()
        };

        return data;
    }

    public async Task<List<ChatAdminEntity>> GetChatAdminByUserId(long userId, bool evictAfter = false)
    {
        var cache = await cacheService.GetOrSetAsync(
            CacheKey.CHAT_ADMIN + userId,
            evictAfter: evictAfter,
            action: async () => {
                var listChatAdmin = await mongoDbContext.ChatAdmin.AsNoTracking()
                    .Where(entity => entity.UserId == userId)
                    .Where(entity => entity.Status == EventStatus.Complete)
                    .ToListAsync();

                return listChatAdmin;
            });

        return cache;
    }

    public async Task<WelcomeMessageEntity?> GetWelcomeMessage(long chatId)
    {
        var welcomeMessage = await mongoDbContext.WelcomeMessage
            .Where(e => e.ChatId == chatId)
            .Where(e => e.Status == EventStatus.Complete)
            .FirstOrDefaultAsync();

        return welcomeMessage;
    }
}
