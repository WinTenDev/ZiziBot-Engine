using MongoFramework.Linq;
using ZiziBot.DataSource.MongoDb;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.DataSource.Repository;

public class MirrorUserRepository(MongoDbContextBase mongoDbContext)
{
    public async Task<MirrorUserEntity?> GetByUserId(long userId)
    {
        var userEntity = await mongoDbContext.MirrorUsers.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Where(x => x.Status == (int)EventStatus.Complete)
            .FirstOrDefaultAsync();

        return userEntity;
    }
}