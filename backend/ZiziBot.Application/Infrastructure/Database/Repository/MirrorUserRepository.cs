using Microsoft.EntityFrameworkCore;
using ZiziBot.Application.Infrastructure.Database.MongoDb;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Infrastructure.Database.Repository;

public class MirrorUserRepository(
    MongoDbContext mongoDbContext
)
{
    public async Task<MirrorUserEntity?> GetByUserId(long userId)
    {
        var userEntity = await mongoDbContext.MirrorUser.AsNoTracking()
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsync();

        return userEntity;
    }

    public async Task<int> SaveActivity(MirrorActivityDto dto)
    {
        await mongoDbContext.MirrorActivity.AddAsync(new() {
            UserId = dto.UserId,
            ActivityTypeId = (int)dto.ActivityTypeId,
            ActivityName = dto.ActivityTypeId.ToString(),
            Url = dto.Url,
            Status = EventStatus.Complete,
            TransactionId = dto.TransactionId,
            CreatedDate = default,
            UpdatedDate = default
        });

        return await mongoDbContext.SaveChangesAsync();
    }

    public async Task<MirrorDonationEntity?> GetDonation(string orderId)
    {
        var mirrorDonationEntity = await mongoDbContext.MirrorDonation.AsNoTracking()
            .Where(x => x.OrderId == orderId)
            .Where(x => x.Status == EventStatus.Complete)
            .FirstOrDefaultAsync();

        return mirrorDonationEntity;
    }
}
