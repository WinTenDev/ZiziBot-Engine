namespace ZiziBot.Application.Infrastructure.Pipelines.PostPipeline;

public interface ISharedPostProcessPipeline<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task ProcessAsync(TRequest request, TResponse response, CancellationToken cancellationToken);
}

public interface ITelegramPostProcessPipeline<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task ProcessAsync(TRequest request, TResponse response, CancellationToken cancellationToken);
}

public interface IRestApiPostProcessPipeline<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task ProcessAsync(TRequest request, TResponse response, CancellationToken cancellationToken);
}
