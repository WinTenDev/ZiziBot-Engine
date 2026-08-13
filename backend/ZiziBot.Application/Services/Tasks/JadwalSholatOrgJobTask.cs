using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Services.Tasks;

public class JadwalSholatOrgJobTask(ILogger<JadwalSholatOrgJobTask> logger, DataFacade dataFacade) : IStartupTask
{
    public async Task ExecuteAsync()
    {
        if (EnvUtil.IsEnabled(Flag.HANGFIRE))
        {
            RecurringJob.AddOrUpdate<JadwalSholatOrgSinkService>(
                CronJobKey.JadwalSholatOrg_FetchAll,
                methodCall: service => service.FeedAll(),
                queue: CronJobKey.Queue_ShalatTime,
                cronExpression: TimeUtil.DayInterval(1)
            );

            var checkCity = await dataFacade.MongoDb.JadwalSholatOrg_City.AsNoTracking()
                .Where(entity => entity.Status == EventStatus.Complete)
                .CountAsync();

            if (checkCity > 0)
            {
                logger.LogInformation("JadwalSholat.org data is already exist, skip seeding data at startup.");
                return;
            }

            RecurringJob.TriggerJob(CronJobKey.JadwalSholatOrg_FetchAll);
        }
    }
}