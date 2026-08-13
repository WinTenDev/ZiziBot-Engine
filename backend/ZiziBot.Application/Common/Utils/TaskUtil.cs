using System.Collections.Concurrent;

namespace ZiziBot.Application.Common.Utils;

public static class TaskUtil
{
    public static async Task<IEnumerable<T>> WhenAll<T>(this IEnumerable<Task<T>> tasks, int parallelCount = int.MaxValue)
    {
        var semaphoreSlim = new SemaphoreSlim(parallelCount);
        var exceptions = new ConcurrentQueue<Exception>();
        var results = new ConcurrentQueue<T>();

        var tasksWithThrottling = tasks.Select(async task =>
        {
            await semaphoreSlim.WaitAsync();

            try
            {
                var result = await task;
                results.Enqueue(result);
            }
            catch (Exception e)
            {
                exceptions.Enqueue(e);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        });

        await Task.WhenAll(tasksWithThrottling);

        if (!exceptions.IsEmpty)
        {
            throw new AggregateException(exceptions);
        }

        return results;
    }
}