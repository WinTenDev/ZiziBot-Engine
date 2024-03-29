using MongoDB.Bson;
using MongoFramework.Linq;

namespace ZiziBot.DataSource.Repository;

public class ChatSettingRepository
{
    private readonly MongoDbContextBase _mongoDbContext;

    public ChatSettingRepository(MongoDbContextBase mongoDbContext)
    {
        _mongoDbContext = mongoDbContext;
    }

    public async Task<WebhookChatEntity?> GetWebhookRouteById(string routeId)
    {
        var webhookChat = await _mongoDbContext.WebhookChat
            .Where(entity => entity.RouteId == routeId)
            .Where(entity => entity.Status == (int)EventStatus.Complete)
            .FirstOrDefaultAsync();
        return webhookChat;
    }

    public async Task<List<ChatInfoDto>?> GetChatByBearerToken(string bearerToken)
    {

        #region Check Dashboard Session
        var dashboardSession = await _mongoDbContext.DashboardSessions
            .Where(entity => entity.BearerToken == bearerToken)
            .Where(entity => entity.Status == (int)EventStatus.Complete)
            .FirstOrDefaultAsync();

        if (dashboardSession == null)
        {
            return null;
        }

        var userId = dashboardSession.TelegramUserId;
        #endregion

        var chatAdmin = await _mongoDbContext.ChatAdmin
            .Where(entity =>
                entity.UserId == userId &&
                entity.Status == (int)EventStatus.Complete
            )
            .ToListAsync();

        if (chatAdmin.Count == 0)
        {
            return null;
        }

        var chatIds = chatAdmin.Select(y => y.ChatId);

        var listChatSetting = await _mongoDbContext.ChatSetting
            .Where(x => chatIds.Contains(x.ChatId))
            .ToListAsync();

        List<ChatInfoDto>? listPermission = new();
        // listPermission.Add(new ChatInfoDto()
        // {
        //     ChatId = request.SessionUserId,
        //     ChatTitle = "Saya"
        // });

        var listGroup = chatAdmin
            .Join(listChatSetting, adminEntity => adminEntity.ChatId,
                settingEntity => settingEntity.ChatId, (adminEntity, settingEntity) => new ChatInfoDto()
                {
                    ChatId = adminEntity.ChatId,
                    ChatTitle = settingEntity.ChatTitle
                })
            .DistinctBy(entity => entity.ChatId)
            .OrderBy(res => res.ChatTitle)
            .ToList();

        listPermission.AddRange(listGroup);

        return listPermission;
    }

    public async Task<List<NoteDto>> GetListNote(long chatId)
    {
        var listNoteEntity = await _mongoDbContext.Note
            .AsNoTracking()
            .Where(entity => entity.ChatId == chatId)
            .Join(_mongoDbContext.ChatSetting, note => note.ChatId, chat => chat.ChatId, (note, chat) => new NoteDto()
            {
                Id = note.Id.ToString(),
                ChatId = note.ChatId,
                ChatTitle = chat.ChatTitle,
                Query = note.Query,
                Text = note.Content,
                RawButton = note.RawButton,
                FileId = note.FileId,
                DataType = note.DataType,
                Status = note.Status,
                TransactionId = note.TransactionId,
                CreatedDate = chat.CreatedDate,
                UpdatedDate = chat.UpdatedDate
            })
            .Where(entity => entity.Status == (int)EventStatus.Complete)
            .OrderBy(entity => entity.Query)
            .ToListAsync();

        return listNoteEntity;
    }

    public async Task<NoteDto> GetNote(string noteId)
    {
        var listNoteEntity = await _mongoDbContext.Note
            .AsNoTracking()
            .Where(entity => entity.Id == new ObjectId(noteId))
            .Join(_mongoDbContext.ChatSetting, note => note.ChatId, chat => chat.ChatId, (note, chat) => new NoteDto()
            {
                Id = note.Id.ToString(),
                ChatId = note.ChatId,
                ChatTitle = chat.ChatTitle,
                Query = note.Query,
                Text = note.Content,
                RawButton = note.RawButton,
                FileId = note.FileId,
                DataType = note.DataType,
                Status = note.Status,
                TransactionId = note.TransactionId,
                CreatedDate = chat.CreatedDate,
                UpdatedDate = chat.UpdatedDate
            })
            .Where(entity => entity.Status == (int)EventStatus.Complete)
            .FirstOrDefaultAsync();

        return listNoteEntity;
    }

    public async Task<List<BangHasan_ShalatCityEntity>> GetShalatCity(long chatId)
    {
        var cityList = await _mongoDbContext.BangHasan_ShalatCity
            .Where(entity => entity.ChatId == chatId)
            .Where(entity => entity.Status == (int)EventStatus.Complete)
            .OrderBy(entity => entity.CityName)
            .ToListAsync();

        return cityList;
    }

    public async Task CreateActivity(ChatActivityDto dto)
    {
        _mongoDbContext.ChatActivity.Add(new ChatActivityEntity
        {
            ChatId = dto.ChatId,
            ActivityType = dto.ActivityType,
            Chat = dto.Chat,
            User = dto.User,
        });

        await _mongoDbContext.SaveChangesAsync();
    }
}