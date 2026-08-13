using Microsoft.EntityFrameworkCore;
using ZiziBot.Application.Infrastructure.Vendor.TonjooStudio;
using ZiziBot.Application.Infrastructure.Database.MongoDb;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Infrastructure.Database.Repository;

public class AdditionalRepository(MongoDbContext mongoDbContextBase)
{
    public async Task<TonjooAwbEntity?> GetStoredAwb(string courier, string awb)
    {
        var awbInfo = await mongoDbContextBase.TonjooAwb
            .Where(entity => entity.Awb == awb)
            .Where(entity => entity.Status == EventStatus.Complete)
            .FirstOrDefaultAsync();

        return awbInfo;
    }

    public async Task SaveAwbInfo(TonjooAwbDetail detail)
    {
        mongoDbContextBase.TonjooAwb.Add(new TonjooAwbEntity() {
            Awb = detail.Code,
            Courier = detail.Kurir.FirstOrDefault("N/A").ToLower(),
            Detail = detail,
            Status = EventStatus.Complete
        });

        await mongoDbContextBase.SaveChangesAsync();
    }
}
