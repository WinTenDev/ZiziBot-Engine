using System.ComponentModel;
using CloudCraic.Hosting.BackgroundQueue;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Services;

public class MediatorService
{
    private readonly ILogger<MediatorService> _logger;
    private readonly IMediator _mediator;
    private readonly IBackgroundQueue _backgroundQueue;

    public MediatorService(ILogger<MediatorService> logger, IMediator mediator, IBackgroundQueue backgroundQueue)
    {
        _logger = logger;
        _mediator = mediator;
        _backgroundQueue = backgroundQueue;
    }

    #region Execution
    public async Task<BotResponseBase> EnqueueAsync(BotRequestBase request)
    {
        BotResponseBase botResponse = new();
        _logger.LogDebug("Enqueueing request {request} in {Mode}", request, request.ExecutionStrategy);

        if (request.ExecutionStrategy == ExecutionStrategy.Instant)
        {
            await _mediator.Send(request);
            return botResponse.Complete();
        }

        switch (request.ExecutionStrategy)
        {
            case ExecutionStrategy.Hangfire:
                BackgroundJob.Enqueue<MediatorService>(x => x.Send(request));
                break;
            case ExecutionStrategy.Background:
                _backgroundQueue.Enqueue(async token => await _mediator.Send(request, token));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(request.ExecutionStrategy), request.ExecutionStrategy, null);
        }

        return botResponse.Complete();
    }

    public BotResponseBase Schedule(BotRequestBase request)
    {
        BotResponseBase botResponse = new();
        BackgroundJob.Schedule<MediatorService>(x => x.Send(request), request.DeleteAfter);
        return botResponse.Complete();
    }
    #endregion

    #region Bridge
    [DisplayName("{0}")]
    [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete, Attempts = 3)]
    public async Task<TResponse?> Send<TResponse>(IRequest<TResponse> request)
    {
        return await _mediator.Send(request);
    }

    [DisplayName("{0}")]
    [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete, Attempts = 3)]
    public async Task<TResponse?> Send<TResponse>(string jobName, IRequest<TResponse> request)
    {
        return await _mediator.Send(request);
    }
    #endregion

}