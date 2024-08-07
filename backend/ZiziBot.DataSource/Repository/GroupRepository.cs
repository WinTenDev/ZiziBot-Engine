﻿using MongoDB.Bson;
using MongoFramework.Linq;
using ZiziBot.DataSource.MongoDb;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.DataSource.Repository;

public class GroupRepository(MongoDbContextBase mongoDbContext, ICacheService cacheService)
{
    public async Task<WelcomeMessageDto?> GetWelcomeMessageById(string welcomeId)
    {
        var query = await mongoDbContext.WelcomeMessage
            .AsNoTracking()
            .Where(entity => entity.Id == new ObjectId(welcomeId))
            .FirstOrDefaultAsync();

        if (query == null)
            return default;

        var listChatSetting = await mongoDbContext.ChatSetting
            .Where(entity => entity.ChatId == query.ChatId)
            .FirstOrDefaultAsync();

        if (listChatSetting == null)
            return default;

        var data = new WelcomeMessageDto() {
            Id = query.Id.ToString(),
            ChatId = query.ChatId,
            ChatTitle = listChatSetting.ChatTitle,
            Text = query.Text,
            RawButton = query.RawButton,
            Media = query.Media,
            DataType = query.DataType,
            DataTypeName = ((CommonMediaType)query.DataType).ToString(),
            Status = query.Status,
            StatusName = ((EventStatus)query.Status).ToString()
        };

        return data;
    }

    public async Task<List<ChatAdminEntity>> GetChatAdminByUserId(long userId, bool evictAfter = false)
    {
        var cache = await cacheService.GetOrSetAsync(
            cacheKey: CacheKey.CHAT_ADMIN + userId,
            evictAfter: evictAfter,
            action: async () => {
                var listChatAdmin = await mongoDbContext.ChatAdmin.AsNoTracking()
                    .Where(entity => entity.UserId == userId)
                    .Where(entity => entity.Status == (int)EventStatus.Complete)
                    .ToListAsync();

                return listChatAdmin;
            });

        return cache;
    }

    public async Task<WelcomeMessageEntity?> GetWelcomeMessage(long chatId)
    {
        var welcomeMessage = await mongoDbContext.WelcomeMessage
            .Where(e => e.ChatId == chatId)
            .Where(e => e.Status == (int)EventStatus.Complete)
            .FirstOrDefaultAsync();

        return welcomeMessage;
    }
}