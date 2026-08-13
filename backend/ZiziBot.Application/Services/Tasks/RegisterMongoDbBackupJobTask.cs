using Hangfire;

namespace ZiziBot.Application.Services.Tasks;

public class RegisterMongoDbBackupJobTask : IStartupTask
{
    public async Task ExecuteAsync()
    {
        if (EnvUtil.IsEnabled(Flag.HANGFIRE))
        {
            RecurringJob.AddOrUpdate<MediatorService>(
                recurringJobId: "mongodb-backup",
                methodCall: service => service.Send(new MongoDbBackupRequest()),
                cronExpression: TimeUtil.DayInterval(1),
                queue: CronJobKey.Queue_Data
            );
        }

        await Task.Delay(1);
    }
}