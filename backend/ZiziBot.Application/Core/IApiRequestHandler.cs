namespace ZiziBot.Application.Core;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IApiRequestHandler<in TRequest, TResponse> : IRequestHandler<TRequest, ApiResponseBase<TResponse>>
    where TRequest : IRequest<ApiResponseBase<TResponse>>
{ }