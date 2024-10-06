namespace ZiziBot.Application.Handlers.Telegram.Group;

public class PromoteMemberBotRequest : BotRequestBase
{
    public bool Promote { get; set; }
}

public class PromoteMemberHandler(
    ServiceFacade serviceFacade
) : IRequestHandler<PromoteMemberBotRequest, BotResponseBase>
{
    public async Task<BotResponseBase> Handle(PromoteMemberBotRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        if (request.Promote)
        {
            if (await serviceFacade.TelegramService.CheckAdministration())
                return await serviceFacade.TelegramService.SendMessageText("Pengguna sudah menjadi admin");

            await serviceFacade.TelegramService.PromoteMember(request.UserId);
            return await serviceFacade.TelegramService.SendMessageText("Promote berhasil");
        }
        else
        {
            if (!await serviceFacade.TelegramService.CheckAdministration())
                return await serviceFacade.TelegramService.SendMessageText("Pengguna sudah bukan lagi admin");

            await serviceFacade.TelegramService.DemoteMember(request.UserId);
            return await serviceFacade.TelegramService.SendMessageText("Demote berhasil");
        }
    }
}