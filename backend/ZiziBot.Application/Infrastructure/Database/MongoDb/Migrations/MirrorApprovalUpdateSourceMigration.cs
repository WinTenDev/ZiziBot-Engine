using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Interfaces;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Migrations;

public class MirrorApprovalUpdateSourceMigration(ILogger<MirrorApprovalUpdateSourceMigration> logger, MongoDbContext mongoDbContext) : IPostMigration
{
    public async Task UpAsync()
    {
        var approvals = await mongoDbContext.MirrorApproval
            .Where(x => x.DonationSource == null || x.DonationSource == DonationSource.Unknown)
            .Where(x => !string.IsNullOrWhiteSpace(x.PaymentUrl))
            .ToListAsync();

        foreach (var approval in approvals)
        {
            logger.LogDebug("Updating donation source for payment {PaymentUrl}", approval.PaymentUrl);

            var source = DonationSource.Unknown;

            if (approval.PaymentUrl?.Contains("trakteer") == true)
            {
                source = DonationSource.Trakteer;
            }
            else if (approval.PaymentUrl?.Contains("saweria") == true)
            {
                source = DonationSource.Saweria;
            }

            approval.DonationSource = source;
            approval.DonationSourceName = source.ToString();
        }

        await mongoDbContext.SaveChangesAsync();
    }

    public Task DownAsync()
    {
        return Task.CompletedTask;
    }
}
