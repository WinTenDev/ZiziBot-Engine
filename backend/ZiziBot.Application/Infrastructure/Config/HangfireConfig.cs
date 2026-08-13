using System.ComponentModel;
using ZiziBot.Application.Common.Constants;

namespace ZiziBot.Application.Infrastructure.Config;

[DisplayName("Hangfire")]
public class HangfireConfig
{
    public string? DashboardTitle { get; set; }
    public CurrentStorage CurrentStorage { get; set; }
    public string? MongoDbConnection { get; set; }
    public int WorkerMultiplier { get; set; } = 2;

    public HangfireQueue[] Queues
    {
        get
        {
            var processorCount = Environment.ProcessorCount;

            return [
                new HangfireQueue() {
                    Name = "default",
                    WorkerCount = WorkerMultiplier * processorCount
                },
                new HangfireQueue() {
                    Name = CronJobKey.Queue_Data,
                    WorkerCount = 10
                },
                new HangfireQueue() {
                    Name = CronJobKey.Queue_Rss,
                    WorkerCount = 2 * WorkerMultiplier * processorCount
                },
                new HangfireQueue() {
                    Name = CronJobKey.Queue_ShalatTime,
                    WorkerCount = 2 * WorkerMultiplier * processorCount
                },
                new HangfireQueue() {
                    Name = CronJobKey.Queue_Telegram,
                    WorkerCount = 20,
                }
            ];
        }
    }
}

public enum CurrentStorage
{
    InMemory,
    MongoDb,
    Sqlite,
    LiteDb
}

public class HangfireQueue
{
    public required string Name { get; set; }
    public int WorkerCount { get; set; }
}