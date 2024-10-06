namespace ZiziBot.Application.Handlers.Telegram.Group.Welcome;

public class ShowWelcomeMessageRequest : BotRequestBase
{ }

public class ShowWelcomeMessageHandler(
    DataFacade dataFacade,
    ServiceFacade serviceFacade
)
    : IRequestHandler<ShowWelcomeMessageRequest, BotResponseBase>
{
    public async Task<BotResponseBase> Handle(ShowWelcomeMessageRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        var welcomeMessage = await dataFacade.Group.GetWelcomeMessage(request.ChatIdentifier);

        if (welcomeMessage == null)
        {
            return await serviceFacade.TelegramService.SendMessageAsync("Belum ada Welcome yang diatur, konfigurasi default akan diterapkan.");
        }

        return await serviceFacade.TelegramService.SendMessageAsync(
            text: welcomeMessage.Text,
            replyMarkup: welcomeMessage.RawButton.ToButtonMarkup(),
            fileId: welcomeMessage.Media,
            mediaType: (CommonMediaType)welcomeMessage.DataType
        );
    }
}