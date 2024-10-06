namespace ZiziBot.Application.Handlers.Telegram.Rss;

public class ResetRssStatusRequest : IRequest<bool>
{ }

public class ResetRssStatusHandler(
    ServiceFacade serviceFacade
) : IRequestHandler<ResetRssStatusRequest, bool>
{
    public async Task<bool> Handle(ResetRssStatusRequest request, CancellationToken cancellationToken)
    {
        await serviceFacade.MediatorService.Send(new RegisterRssJobAllRequest() {
            ResetStatus = true
        });

        return true;
    }
}