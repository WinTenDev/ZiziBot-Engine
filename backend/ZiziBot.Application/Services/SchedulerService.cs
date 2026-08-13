using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Services;

public class SchedulerService(
    ILogger<SchedulerService> logger,
    IRecurringJobManager recurringJobManager,
    IBackgroundJobClient backgroundJobClient
)
{
    public void Recurring<T>(
        string recurringJobId,
        Expression<Func<T, Task>> methodCall,
        string cronExpression
    )
    {
        recurringJobManager.AddOrUpdate(recurringJobId, methodCall, cronExpression, new RecurringJobOptions());
    }

    public void Enqueue<T>(Expression<Func<T, Task>> methodCall)
    {
        if (!EnvUtil.IsEnabled(Flag.HANGFIRE))
            return;

        backgroundJobClient.Enqueue(methodCall);
    }

    public void Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay)
    {
        if (!EnvUtil.IsEnabled(Flag.HANGFIRE))
            return;

        backgroundJobClient.Schedule(methodCall, delay);
    }
}