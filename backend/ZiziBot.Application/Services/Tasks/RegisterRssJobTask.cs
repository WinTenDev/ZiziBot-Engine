using Hangfire;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Features.Hangfire;
using ZiziBot.Application.Infrastructure.Scheduler;
using ZiziBot.Application.Features.UseCases.GitHub;
using ZiziBot.Application.Features.UseCases.Rss;

namespace ZiziBot.Application.Services.Tasks;

public class RegisterRssJobTask(
    ILogger<RegisterRssJobTask> logger,
    RegisterRssJobAllUseCase registerRssJobAllUseCase,
    AppSettingRepository appSettingRepository,
    FeatureFlagRepository featureFlagRepository,
    SchedulerService schedulerService
) : IStartupTask
{
    public async Task ExecuteAsync()
    {
        if (EnvUtil.IsEnabled(Flag.HANGFIRE))
        {
            if (EnvUtil.IsEnabled(Flag.RSS_BROADCASTER))
            {
                logger.LogInformation("Registering RSS Jobs");

                await registerRssJobAllUseCase.Handle(new RegisterRssJobAllRequest()
                {
                    ResetStatus = await featureFlagRepository.IsEnabled(Flag.RSS_RESET_AT_STARTUP)
                });

                schedulerService.Recurring<RssScheduler>(
                    recurringJobId: CronJobKey.Rss_Reset,
                    methodCall: x => x.RssRefresh(),
                    cronExpression: TimeUtil.HourInterval(1)
                );

                schedulerService.Recurring<UpdateStatisticUseCase>(
                    recurringJobId: CronJobKey.GitHub_Update_Token,
                    methodCall: x => x.Handle(),
                    cronExpression: TimeUtil.MinuteInterval(3)
                );

                logger.LogDebug("Registering RSS Jobs Completed");
            }
            else
            {
                RecurringJob.RemoveIfExists(CronJobKey.Rss_Reset);
                HangfireUtil.RemoveRssJobs();
            }
        }
    }
}