using Hangfire;

namespace ZiziBot.Application.Services.Tasks;

public class HangfireCleanupTask : IStartupTask
{
    public Task ExecuteAsync()
    {
        if (EnvUtil.IsEnabled(Flag.HANGFIRE))
        {
            var api = JobStorage.Current.GetMonitoringApi();
            var processingJobs = api.ProcessingJobs(0, int.MaxValue);
            var servers = api.Servers();

            #region Requeue orphan jobs
            var orphanJobs = processingJobs
                .Where(job => servers.All(server => server.Name != job.Value.ServerId));

            foreach (var orphanJob in orphanJobs)
            {
                BackgroundJob.Requeue(orphanJob.Key);
            }
            #endregion

            #region Remove Enqueued
            var toDelete = new List<string>();

            foreach (var queue in api.Queues())
            {
                for (var i = 0; i < Math.Ceiling(queue.Length / 1000d); i++)
                {
                    api.EnqueuedJobs(queue.Name, 1000 * i, 1000).ForEach(x => toDelete.Add(x.Key));
                }
            }

            foreach (var jobId in toDelete)
            {
                BackgroundJob.Delete(jobId);
            }
            #endregion
        }

        return Task.CompletedTask;
    }
}