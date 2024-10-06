using MongoFramework.Linq;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.Application.Services;

public class SudoService(MongoDbContextBase mongoDbContext, CacheService cacheService)
{
    public async Task<bool> IsSudoAsync(long userId)
    {
        var cache = await cacheService.GetOrSetAsync(
            cacheKey: CacheKey.GLOBAL_SUDO + userId,
            action: async () => {
                return await mongoDbContext.Sudoers.AnyAsync(
                    x =>
                        x.UserId == userId &&
                        x.Status == (int)EventStatus.Complete
                );
            }
        );

        return cache;
    }

    public async Task<ServiceResult> SaveSudo(SudoerEntity entity)
    {
        ServiceResult serviceResult = new();

        var findSudo = await mongoDbContext.Sudoers
            .FirstOrDefaultAsync(x => x.UserId == entity.UserId);

        if (findSudo != null)
        {
            return serviceResult.Complete("This user is already a sudoer.");
        }

        mongoDbContext.Sudoers.Add(entity);
        await mongoDbContext.SaveChangesAsync();

        return serviceResult.Complete("Sudoer added successfully.");
    }
}