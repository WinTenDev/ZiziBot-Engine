using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Services.Tasks;

public class SeedFlagTask(
    ILogger<SeedFlagTask> logger,
    DataFacade dataFacade
) : IStartupTask
{
    public async Task ExecuteAsync()
    {
        logger.LogInformation("");

        var transactionId = Guid.NewGuid().ToString();
        var listFlags = Flag.GetFields();

        foreach (var flag in listFlags)
        {
            var featureFlagEntity = await dataFacade.MongoDb.FeatureFlag
                .Where(x => x.Name == flag.Name)
                .Where(x => x.Status == EventStatus.Complete)
                .FirstOrDefaultAsync();

            if (featureFlagEntity == null)
            {
                dataFacade.MongoDb.FeatureFlag.Add(new() {
                    Name = flag.Name,
                    IsEnabled = flag.Value,
                    Status = EventStatus.Complete,
                    TransactionId = transactionId
                });
            }
        }

        await dataFacade.MongoDb.SaveChangesAsync();
    }
}