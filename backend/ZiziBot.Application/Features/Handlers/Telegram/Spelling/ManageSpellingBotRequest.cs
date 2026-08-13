using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Features.Handlers.Telegram.Spelling;

public class ManageSpellingBotRequest : BotRequestBase
{
}

public class ManageSpellingHandler(
    ILogger<ManageSpellingHandler> logger,
    DataFacade dataFacade,
    ServiceFacade serviceFacade
)
    : IBotRequestHandler<ManageSpellingBotRequest>
{
    public async Task<BotResponseBase> Handle(ManageSpellingBotRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        return await serviceFacade.TelegramService.SendMessageAsync("Spelling handler belum diimplementasikan");
    }
}