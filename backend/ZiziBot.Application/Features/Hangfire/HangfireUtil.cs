using System.Linq.Expressions;
using Hangfire.Storage;
using Serilog;

namespace ZiziBot.Application.Features.Hangfire;

public static class HangfireUtil
{
    public static List<RecurringJobDto> GetRssJobs()
    {
        Log.Information("Collecting previous RSS Jobs..");
        var storageConnection = JobStorage.Current.GetConnection();
        var recurringJobs = storageConnection.GetRecurringJobs();
        var rssJobs = recurringJobs.Where(dto => dto.Id.StartsWith(CronJobKey.Rss_Prefix + ":", StringComparison.InvariantCultureIgnoreCase)).ToList();
        Log.Debug("Found {Count} RSS Jobs", rssJobs.Count);

        return rssJobs;
    }

    public static void RemoveRssJobs()
    {
        var rssJobs = GetRssJobs();

        rssJobs.ForEach(job =>
        {
            Log.Debug("Deleting RSS Job: {Id}", job.Id);
            RecurringJob.RemoveIfExists(job.Id);
        });

        Log.Information("Deleting RSS {Count} Jobs finish..", rssJobs.Count);
    }

    public static void RemoveRecurringJob(string jobId)
    {
        try
        {
            Log.Debug("Removing Recurring Job: {JobId}", jobId);
            RecurringJob.RemoveIfExists(jobId);
        }
        catch (Exception e)
        {
            Log.Error("Error removing recurring job: {JobId}. Message: {Message}", jobId, e.Message);
        }
    }

    public static void Enqueue<T>(Expression<Func<T, Task>> methodCall)
    {
        if (!EnvUtil.IsEnabled(Flag.HANGFIRE))
        {
            var compile = methodCall.Compile();
            var instance = Activator.CreateInstance<T>();
            _ = compile.Invoke(instance);

            return;
        }

        BackgroundJob.Enqueue<T>(methodCall);
    }

    public static void Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay)
    {
        if (!EnvUtil.IsEnabled(Flag.HANGFIRE))
            return;

        BackgroundJob.Schedule(methodCall, delay);
    }
}