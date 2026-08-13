using System.Reflection;
using ZiziBot.Application.Infrastructure.Messaging;

namespace ZiziBot.Application.Infrastructure.Pipelines;

public sealed class CortexAppMediator(IMediator mediator) : IAppMediator
{
    private static readonly MethodInfo SendQueryAsyncMethod = typeof(IMediator)
        .GetMethods()
        .Single(method => method.Name == "SendQueryAsync" && method.IsGenericMethodDefinition && method.GetGenericArguments().Length == 2);

    public Task<TResponse> SendAsync<TResponse>(IAppCommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        var sendAsync = SendQueryAsyncMethod.MakeGenericMethod(command.GetType(), typeof(TResponse));
        return (Task<TResponse>)sendAsync.Invoke(mediator, [command, cancellationToken])!;
    }

    public Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : IAppNotification
    {
        return mediator.PublishAsync(notification, cancellationToken);
    }
}