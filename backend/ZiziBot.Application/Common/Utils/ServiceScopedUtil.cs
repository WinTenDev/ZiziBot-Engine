using Microsoft.Extensions.DependencyInjection;

namespace ZiziBot.Application.Common.Utils;

public interface IServiceScopedUtil
{
    void Execute(Action<IServiceProvider> action);
    Task ExecuteAsync(Func<IServiceProvider, Task> action);
    Task<T> ExecuteAsync<T>(Func<IServiceProvider, Task<T>> action);
}

public class ServiceScopedUtil(IServiceScopeFactory scopeFactory) : IServiceScopedUtil
{
    public void Execute(Action<IServiceProvider> action)
    {
        using var scope = scopeFactory.CreateScope();
        action(scope.ServiceProvider);
    }

    public async Task ExecuteAsync(Func<IServiceProvider, Task> action)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        await action(scope.ServiceProvider);
    }

    public async Task<T> ExecuteAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        return await action(scope.ServiceProvider);
    }
}