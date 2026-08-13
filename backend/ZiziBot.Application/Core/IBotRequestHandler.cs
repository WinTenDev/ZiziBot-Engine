namespace ZiziBot.Application.Core;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IBotRequestHandler<TRequest>
    : IAppCommandHandler<TRequest, BotResponseBase>
    where TRequest : BotRequestBase
{
}
