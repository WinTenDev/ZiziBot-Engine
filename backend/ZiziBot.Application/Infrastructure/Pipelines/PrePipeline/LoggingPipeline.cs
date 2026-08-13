using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Infrastructure.Pipelines.PrePipeline;

public class LoggingPipeline<TRequest, TResponse>(ILogger<LoggingPipeline<TRequest, TResponse>> logger)
    : IQueryPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, QueryHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var sessionId = Guid.NewGuid().ToString("N");
        var requestType = PipelineMetrics.GetTypeName(typeof(TRequest));
        var responseType = PipelineMetrics.GetTypeName(typeof(TResponse));
        var stopwatch = Stopwatch.StartNew();
        PipelineMetrics.CurrentSessionId = sessionId;

        try
        {
            logger.LogDebug(
                "PipelineMetrics Stage={Stage} SessionId={SessionId} RequestType={RequestType} ResponseType={ResponseType} Duration={DurationMs} Status={Status}",
                "Request",
                sessionId,
                requestType,
                responseType,
                0,
                "Started"
            );

            var result = await next();
            stopwatch.Stop();

            logger.LogInformation(
                "PipelineMetrics Stage={Stage} SessionId={SessionId} RequestType={RequestType} ResponseType={ResponseType} Duration={DurationMs} Status={Status}",
                "Request",
                sessionId,
                requestType,
                responseType,
                stopwatch.Elapsed.ToString("mm\\:ss\\.fff"),
                "Completed"
            );

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(
                ex,
                "PipelineMetrics Stage={Stage} SessionId={SessionId} RequestType={RequestType} ResponseType={ResponseType} Duration={DurationMs} Status={Status} Message={Message}",
                "Request",
                sessionId,
                requestType,
                responseType,
                stopwatch.Elapsed.ToString("mm\\:ss\\.fff"),
                "Failed",
                ex.Message
            );
            throw;
        }
        finally
        {
            PipelineMetrics.CurrentSessionId = null;
        }
    }
}
