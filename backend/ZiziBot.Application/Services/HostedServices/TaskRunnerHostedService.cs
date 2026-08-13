using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Services.HostedServices;

public class TaskRunnerHostedService(IServiceScopedUtil scopedUtil, ILogger<TaskRunnerHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (EnvUtil.GetEnv("ASPNETCORE_ENVIRONMENT") == "UnitTest")
        {
            logger.LogWarning("Skip triggering startup tasks in unit test environment");
            return;
        }

        await scopedUtil.ExecuteAsync(async provider =>
        {
            var services = provider.GetServices<IStartupTask>().ToList();

            foreach (var service in services)
            {
                var task = service.GetType();

                try
                {
                    logger.LogInformation("Executing task: {Task}", task);
                    await service.ExecuteAsync();

                    logger.LogInformation("Task executed: {Task}", task);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "An error occured when executing task {Task}", task);
                }

                logger.LogInformation("Task triggered: {Task}", task);
            }

            await Task.Delay(0, stoppingToken);

            logger.LogInformation("All tasks are triggered. Count: {Count}", services.Count);
        });
    }
}