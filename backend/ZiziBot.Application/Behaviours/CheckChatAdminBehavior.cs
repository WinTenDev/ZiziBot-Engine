namespace ZiziBot.Application.Behaviours;

public class CheckChatAdminBehavior(
    ServiceFacade serviceFacade
) : IPipelineBehavior<BotRequestBase, BotResponseBase>
{
    public async Task<BotResponseBase> Handle(BotRequestBase request, RequestHandlerDelegate<BotResponseBase> next, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        var checkAdministration = await serviceFacade.TelegramService.CheckAdministration();

        if (checkAdministration)
        {
            return await next();
        }

        throw new UnauthorizedAccessException($"UserId: {request.UserId} is not a Administrator in ChatId: {request.ChatIdentifier} ");
    }
}