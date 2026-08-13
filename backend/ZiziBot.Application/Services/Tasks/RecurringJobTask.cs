using Hangfire;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Features.UseCases.User;

namespace ZiziBot.Application.Services.Tasks;

public class RecurringJobTask(ILogger<RecurringJobTask> logger) : IStartupTask
{
    public Task ExecuteAsync()
    {
        if (EnvUtil.IsEnabled(Flag.HANGFIRE))
        {
            logger.LogInformation("Register Job session-cleaner");
            RecurringJob.AddOrUpdate<SessionCleanerUseCase>(
                recurringJobId: "session-cleaner",
                methodCall: x => x.Handle(),
                cronExpression: TimeUtil.DayInterval(1),
                queue: CronJobKey.Queue_Data
            );
        }

        return Task.CompletedTask;
    }
}