using Microsoft.Extensions.Logging;
using MongoFramework.Linq;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.Application.Services;

public class NoteService(ILogger<NoteService> logger, MongoDbContextBase mongoDbContext, CacheService cacheService)
{
    public async Task<List<NoteDto>> GetAllByChat(long chatId, bool evictBefore = false)
    {
        var cache = await cacheService.GetOrSetAsync(
            cacheKey: CacheKey.CHAT_NOTES + chatId,
            evictBefore: evictBefore,
            action: async () => {
                var noteEntities = await mongoDbContext.Note
                    .Where(entity => entity.ChatId == chatId)
                    .Where(entity => entity.Status == (int)EventStatus.Complete)
                    .OrderBy(entity => entity.Query)
                    .ToListAsync();

                var noteDto = noteEntities.Select(entity => new NoteDto {
                    Id = entity.Id.ToString() ?? string.Empty,
                    ChatId = entity.ChatId,
                    Query = entity.Query,
                    Text = entity.Content,
                    RawButton = entity.RawButton,
                    FileId = entity.FileId,
                    DataType = entity.DataType,
                    Status = entity.Status,
                    TransactionId = entity.TransactionId,
                    CreatedDate = entity.CreatedDate,
                    UpdatedDate = entity.UpdatedDate
                }).ToList();

                return noteDto;
            });

        return cache;
    }

    public async Task<ServiceResult> Save(NoteEntity entity)
    {
        ServiceResult result = new();
        logger.LogInformation("Checking Note with Query: {Query}", entity.Query);

        var findNote = await mongoDbContext.Note
            .Where(x => x.Id == entity.Id)
            .Where(x => x.ChatId == entity.ChatId)
            .FirstOrDefaultAsync();

        if (findNote == null)
        {
            logger.LogInformation("Adding Note with Query: {Query}", entity.Query);
            mongoDbContext.Note.Add(entity);

            result.Message = "Note created successfully";
        }
        else
        {
            logger.LogInformation("Updating Note with Id: {Id}", entity.Id);

            findNote.Query = entity.Query;
            findNote.Content = entity.Content;
            findNote.DataType = entity.DataType;
            findNote.FileId = entity.FileId;
            findNote.RawButton = entity.RawButton;
            findNote.TransactionId = entity.TransactionId;
            findNote.UserId = entity.UserId;

            result.Message = "Note updated successfully";
        }

        await mongoDbContext.SaveChangesAsync();

        await GetAllByChat(entity.ChatId, true);

        return result;
    }

    public async Task<BotResponseBase> Delete(long chatId, string note, Func<string, Task<BotResponseBase>> func)
    {
        var findNote = await mongoDbContext.Note
            .Where(x => x.ChatId == chatId)
            .Where(x => x.Query == note)
            .Where(x => x.Status == (int)EventStatus.Complete)
            .FirstOrDefaultAsync();

        if (findNote == null)
            return await func.Invoke("Note tidak ditemukan, mungkin sudah dihapus.");

        findNote.Status = (int)EventStatus.Deleted;

        await mongoDbContext.SaveChangesAsync();

        return await func.Invoke("Note berhasil dihapus");
    }
}