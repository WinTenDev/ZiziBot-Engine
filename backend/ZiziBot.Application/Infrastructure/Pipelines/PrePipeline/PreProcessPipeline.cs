using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Infrastructure.Pipelines.PrePipeline;

public class PreProcessPipeline<TRequest, TResponse>(
    IEnumerable<ISharedPreProcessPipeline<TRequest, TResponse>> sharedPreProcessors,
    IEnumerable<ITelegramPreProcessPipeline<TRequest, TResponse>> telegramPreProcessors,
    IEnumerable<IRestApiPreProcessPipeline<TRequest, TResponse>> restApiPreProcessors,
    ILogger<PreProcessPipeline<TRequest, TResponse>> logger
) : IQueryPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        QueryHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var sessionId = PipelineMetrics.CurrentSessionId ?? "none";
        var requestType = PipelineMetrics.GetTypeName(typeof(TRequest));
        var processorList = BuildProcessorList(request);
        var stageStopwatch = Stopwatch.StartNew();

        logger.LogDebug(
            "PipelineMetrics Stage={Stage} SessionId={SessionId} RequestType={RequestType} ProcessorCount={ProcessorCount} Duration={DurationMs} Status={Status}",
            "Pre",
            sessionId,
            requestType,
            processorList.Count,
            0,
            "Started"
        );

        foreach (var (preProcessor, index) in processorList.Select((processor, processorIndex) =>
                     (processor, processorIndex + 1)))
        {
            var processorType = preProcessor.Name;
            var processorStopwatch = Stopwatch.StartNew();
            var result = await preProcessor.Execute(request, cancellationToken);
            processorStopwatch.Stop();

            logger.LogInformation(
                "PipelineMetrics Stage={Stage} SessionId={SessionId} RequestType={RequestType} Pipeline={Pipeline} Index={Index} Duration={DurationMs} ShouldContinue={ShouldContinue} Status={Status}",
                "Pre",
                sessionId,
                requestType,
                processorType,
                index,
                processorStopwatch.Elapsed.ToString("mm\\:ss\\.fff"),
                result.ShouldContinue,
                result.ShouldContinue ? "Completed" : "ShortCircuited"
            );

            if (!result.ShouldContinue)
            {
                stageStopwatch.Stop();
                logger.LogInformation(
                    "PipelineMetrics Stage={Stage} SessionId={SessionId} RequestType={RequestType} ProcessorCount={ProcessorCount} Duration={Duration} Status={Status}",
                    "Pre",
                    sessionId,
                    requestType,
                    processorList.Count,
                    stageStopwatch.Elapsed.ToString("mm\\:ss\\.fff"),
                    "ShortCircuited"
                );

                return result.Response!;
            }
        }

        stageStopwatch.Stop();
        logger.LogInformation(
            "PipelineMetrics Stage={Stage} SessionId={SessionId} RequestType={RequestType} ProcessorCount={ProcessorCount} Duration={Duration} Status={Status}",
            "Pre",
            sessionId,
            requestType,
            processorList.Count,
            stageStopwatch.Elapsed.ToString("mm\\:ss\\.fff"),
            "Completed"
        );

        return await next();
    }

    private List<PreProcessorRegistration> BuildProcessorList(TRequest request)
    {
        var processors = sharedPreProcessors
            .Select(processor => new PreProcessorRegistration(
                PipelineMetrics.GetTypeName(processor.GetType()),
                processor.ProcessAsync))
            .ToList();

        if (request is ITelegramRequest)
        {
            processors.AddRange(telegramPreProcessors.Select(processor => new PreProcessorRegistration(
                PipelineMetrics.GetTypeName(processor.GetType()),
                processor.ProcessAsync)));
        }

        if (request is IRestApiRequest)
        {
            processors.AddRange(restApiPreProcessors.Select(processor => new PreProcessorRegistration(
                PipelineMetrics.GetTypeName(processor.GetType()),
                processor.ProcessAsync)));
        }

        return processors;
    }

    private sealed record PreProcessorRegistration(
        string Name,
        Func<TRequest, CancellationToken, Task<PreProcessResult<TResponse>>> Execute
    );
}
