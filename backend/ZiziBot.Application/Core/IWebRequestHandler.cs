namespace ZiziBot.Application.Core;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IWebRequestHandler<TRequest, TResponse>
    : IAppCommandHandler<TRequest, WebResponseBase<TResponse>>
    where TRequest : IAppCommand<WebResponseBase<TResponse>>
{ }
