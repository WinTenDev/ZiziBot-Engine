using System.Reflection;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Common.Utils;

namespace ZiziBot.Application.Infrastructure.Pipelines.PrePipeline;

public class FeatureControlPipeline<TRequest, TResponse>(
    ILogger<FeatureControlPipeline<TRequest, TResponse>> logger,
    DataFacade dataFacade
) : ITelegramPreProcessPipeline<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<PreProcessResult<TResponse>> ProcessAsync(TRequest request, CancellationToken cancellationToken)
    {
        if (request is not BotRequestBase botRequest)
            return PreProcessResult<TResponse>.Continue;

        var requestType = request.GetType();
        var featureName = requestType.GetCustomAttribute<FeatureFlagAttribute>()?.FeatureName ?? string.Empty;

        var featureFlag = await dataFacade.FeatureFlag.GetFlag(featureName);

        if (featureFlag == null ||
            featureFlag.IsEnabled)
            return PreProcessResult<TResponse>.Continue;

        logger.LogWarning("Feature {FeatureName} is disabled by global flag", featureName);

        if (typeof(TResponse).IsAssignableTo(typeof(BotResponseBase)))
        {
            var response = new BotResponseBase();
            return PreProcessResult<TResponse>.Stop((TResponse)Convert.ChangeType(response, typeof(TResponse)));
        }

        return PreProcessResult<TResponse>.Stop();
    }
}
