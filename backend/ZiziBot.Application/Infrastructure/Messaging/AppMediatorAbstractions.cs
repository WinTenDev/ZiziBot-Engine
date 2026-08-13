namespace ZiziBot.Application.Infrastructure.Messaging;

public interface IRequest<out TResponse> : IQuery<TResponse>
{
}

public interface IRequestHandler<TRequest, TResponse> : IQueryHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
}

public interface INotification : Cortex.Mediator.Notifications.INotification
{
}

public interface INotificationHandler<TNotification> : Cortex.Mediator.Notifications.INotificationHandler<TNotification>
    where TNotification : INotification
{
}

public interface IAppCommand<out TResponse> : IRequest<TResponse>
{
}

public interface IAppNotification : INotification
{
}

public interface IAppCommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : IAppCommand<TResponse>
{
}

public interface IAppNotificationHandler<TNotification> : INotificationHandler<TNotification>
    where TNotification : IAppNotification
{
}

public interface IAppMediator
{
    Task<TResponse> SendAsync<TResponse>(IAppCommand<TResponse> command, CancellationToken cancellationToken = default);
    Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : IAppNotification;
}
