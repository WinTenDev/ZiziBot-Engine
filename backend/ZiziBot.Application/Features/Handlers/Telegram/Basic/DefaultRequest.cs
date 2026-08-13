namespace ZiziBot.Application.Features.Handlers.Telegram.Basic;

public class DefaultBotRequestModel : BotRequestBase
{ }

public class DefaultRequestHandler(
    ServiceFacade serviceFacade
) : IBotRequestHandler<DefaultBotRequestModel>
{
    public async Task<BotResponseBase> Handle(DefaultBotRequestModel request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        await Task.Delay(1, cancellationToken);

        return serviceFacade.TelegramService.Complete();
    }
}