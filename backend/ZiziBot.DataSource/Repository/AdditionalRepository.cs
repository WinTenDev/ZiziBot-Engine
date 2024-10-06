using MongoFramework.Linq;
using ZiziBot.DataSource.MongoDb;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.DataSource.Repository;

public class AdditionalRepository(MongoDbContextBase mongoDbContextBase)
{
    public async Task<TonjooAwbEntity?> GetStoredAwb(string courier, string awb)
    {
        var awbInfo = await mongoDbContextBase.TonjooAwb
            .Where(entity => entity.Awb == awb)
            .Where(entity => entity.Status == (int)EventStatus.Complete)
            .FirstOrDefaultAsync();

        return awbInfo;
    }

    public async Task SaveAwbInfo(TonjooAwbDetail detail)
    {
        mongoDbContextBase.TonjooAwb.Add(new TonjooAwbEntity() {
            Awb = detail.Code,
            Courier = detail.Kurir.FirstOrDefault("N/A").ToLower(),
            Detail = detail,
            Status = (int)EventStatus.Complete
        });

        await mongoDbContextBase.SaveChangesAsync();
    }
}