namespace ZiziBot.Application.Core;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IApiRequestHandler<TRequest, TResponse>
    : IAppCommandHandler<TRequest, ApiResponseBase<TResponse>>
    where TRequest : IAppCommand<ApiResponseBase<TResponse>>
{ }
