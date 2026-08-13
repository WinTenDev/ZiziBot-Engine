using System.ComponentModel;
using DalSoft.Hosting.BackgroundQueue;
using Hangfire;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Common.Exceptions;

namespace ZiziBot.Application.Services;

public class MediatorService(
    ILogger<MediatorService> logger,
    IAppMediator mediator,
    IBackgroundQueue backgroundQueue
)
{
    #region Execution

    public async Task<BotResponseBase> EnqueueAsync(BotRequestBase request)
    {
        BotResponseBase botResponse = new();
        logger.LogDebug("Enqueueing request {Request} in {Mode}", request, request.ExecutionStrategy);

        switch (request.ExecutionStrategy)
        {
            case ExecutionStrategy.Hangfire:
                var jobId = $"{request.Source}-{request.ChatId}-{request.MessageId}-{request.UserId}-{request.Command}";
                BackgroundJob.Enqueue<MediatorService>(x => x.Send(jobId, request));

                break;
            case ExecutionStrategy.Background:
                backgroundQueue.Enqueue(async token => await mediator.SendAsync(request, token));

                break;
            case ExecutionStrategy.Instant:
                return await mediator.SendAsync(request);
            default:
                throw new AppException("Unknown execution strategy");
        }

        return botResponse.Complete();
    }

    public async Task<ApiResponseBase<T>> EnqueueAsync<T>(ApiRequestBase<T> request, ExecutionStrategy executionStrategy = default)
    {
        switch (executionStrategy)
        {
            case ExecutionStrategy.Hangfire:
                BackgroundJob.Enqueue<MediatorService>(x => x.Send(request));

                return new();

            case ExecutionStrategy.Instant:
                return await mediator.SendAsync(request);
            default:
                throw new ArgumentOutOfRangeException(nameof(executionStrategy));
        }
    }

    public async Task<T> EnqueueAsync<T>(IAppCommand<T> request, ExecutionStrategy executionStrategy = default) where T : new()
    {
        switch (executionStrategy)
        {
            case ExecutionStrategy.Hangfire:
                BackgroundJob.Enqueue<MediatorService>(x => x.Send(request));

                return new();

            case ExecutionStrategy.Instant:
                return await mediator.SendAsync(request);
            default:
                throw new ArgumentOutOfRangeException(nameof(executionStrategy));
        }
    }

    public BotResponseBase Schedule(BotRequestBase request, TimeSpan delayExecution = default)
    {
        BotResponseBase botResponse = new();
        BackgroundJob.Schedule<MediatorService>(x => x.Send(request), delayExecution);

        return botResponse.Complete();
    }

    #endregion

    #region Bridge

    [DisplayName("{0}")]
    [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete, Attempts = 3)]
    public async Task<TResponse?> Send<TResponse>(IAppCommand<TResponse> request)
    {
        return await mediator.SendAsync(request);
    }

    [DisplayName("{0}")]
    [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete, Attempts = 3)]
    public async Task<TResponse?> Send<TResponse>(string jobName, IAppCommand<TResponse> request)
    {
        return await mediator.SendAsync(request);
    }

    #endregion
}
