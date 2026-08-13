using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Infrastructure.Pipelines.PostPipeline;

public class PostProcessPipeline<TRequest, TResponse>(
    IEnumerable<ISharedPostProcessPipeline<TRequest, TResponse>> sharedPostProcessors,
    IEnumerable<ITelegramPostProcessPipeline<TRequest, TResponse>> telegramPostProcessors,
    IEnumerable<IRestApiPostProcessPipeline<TRequest, TResponse>> restApiPostProcessors,
    ILogger<PostProcessPipeline<TRequest, TResponse>> logger
) : IQueryPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, QueryHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var sessionId = PipelineMetrics.CurrentSessionId ?? "none";
        var requestType = PipelineMetrics.GetTypeName(typeof(TRequest));
        var processorList = BuildProcessorList(request);
        var response = await next();
        var stageStopwatch = Stopwatch.StartNew();

        logger.LogDebug(
            "PipelineMetrics Stage={Stage} SessionId={SessionId} RequestType={RequestType} ProcessorCount={ProcessorCount} Duration={DurationMs} Status={Status}",
            "Post",
            sessionId,
            requestType,
            processorList.Count,
            0,
            "Started"
        );

        foreach (var (postProcessor, index) in processorList.Select((processor, processorIndex) => (processor, processorIndex + 1)))
        {
            var processorType = postProcessor.Name;
            var processorStopwatch = Stopwatch.StartNew();
            await postProcessor.Execute(request, response, cancellationToken);
            processorStopwatch.Stop();

            logger.LogInformation(
                "PipelineMetrics Stage={Stage} SessionId={SessionId} RequestType={RequestType} Pipeline={Pipeline} Index={Index} Duration={DurationMs} Status={Status}",
                "Post",
                sessionId,
                requestType,
                processorType,
                index,
                processorStopwatch.Elapsed.ToString("mm\\:ss\\.fff"),
                "Completed"
            );
        }

        stageStopwatch.Stop();
        logger.LogInformation(
            "PipelineMetrics Stage={Stage} SessionId={SessionId} RequestType={RequestType} ProcessorCount={ProcessorCount} Duration={DurationMs} Status={Status}",
            "Post",
            sessionId,
            requestType,
            processorList.Count,
            stageStopwatch.Elapsed.ToString("mm\\:ss\\.fff"),
            "Completed"
        );

        return response;
    }

    private List<PostProcessorRegistration> BuildProcessorList(TRequest request)
    {
        var processors = sharedPostProcessors
            .Select(processor => new PostProcessorRegistration(
                PipelineMetrics.GetTypeName(processor.GetType()),
                processor.ProcessAsync))
            .ToList();

        if (request is ITelegramRequest)
        {
            processors.AddRange(telegramPostProcessors.Select(processor => new PostProcessorRegistration(
                PipelineMetrics.GetTypeName(processor.GetType()),
                processor.ProcessAsync)));
        }

        if (request is IRestApiRequest)
        {
            processors.AddRange(restApiPostProcessors.Select(processor => new PostProcessorRegistration(
                PipelineMetrics.GetTypeName(processor.GetType()),
                processor.ProcessAsync)));
        }

        return processors;
    }

    private sealed record PostProcessorRegistration(
        string Name,
        Func<TRequest, TResponse, CancellationToken, Task> Execute
    );
}
