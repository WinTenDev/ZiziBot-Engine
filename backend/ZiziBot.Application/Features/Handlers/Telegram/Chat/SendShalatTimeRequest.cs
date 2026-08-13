using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace ZiziBot.Application.Features.Handlers.Telegram.Chat;

[Obsolete("Please use from UseCase")]
public class SendShalatTimeRequest : IAppCommand<bool>, IRequest<bool>
{
    public long ChatId { get; set; }
}

public class SendShalatTimeHandler(
    ILogger<SendShalatTimeHandler> logger,
    DataFacade dataFacade,
    ServiceFacade serviceFacade
)
    : IRequestHandler<SendShalatTimeRequest, bool>
{
    public async Task<bool> Handle(SendShalatTimeRequest request, CancellationToken cancellationToken)
    {
        var botMain = await dataFacade.Bot.GetBotMain();
        var botClient = new TelegramBotClient(botMain.Token);

        const string defaultMessage = "Telah masuk waktu <b>{Shalat}</b> untuk wilayah <b>{City}</b> dan sekitarnya.";

        var cityList = await dataFacade.ChatSetting.GetShalatCity(request.ChatId);

        if (cityList.IsEmpty())
        {
            logger.LogInformation("City list is empty for ChatId: {ChatId}", request.ChatId);
            return false;
        }

        logger.LogDebug("Found about {Count} city(es) for ChatId: {ChatId}", cityList.Count, request.ChatId);

        foreach (var cityEntity in cityList)
        {
            try
            {
                var currentShalat = await serviceFacade.FathimahRestService.GetCurrentShalatTime(cityEntity.CityId);

                if (currentShalat.IsNullOrDefault())
                {
                    logger.LogDebug("No match Shalat time for city: '{CityName}' at '{CurrentTime}'",
                        cityEntity.CityName, DateTime.UtcNow.AddHours(Env.DEFAULT_TIMEZONE).ToString("HH:mm"));

                    continue;
                }

                var message = defaultMessage.ResolveVariable(new List<(string placeholder, string value)> {
                    ("Shalat", currentShalat?.Key),
                    ("Date", DateTime.UtcNow.AddHours(Env.DEFAULT_TIMEZONE).ToString("yyyy-MM-dd HH:mm:ss")),
                    ("City", cityEntity.CityName)
                });

                await botClient.SendMessage(request.ChatId, message, parseMode: ParseMode.Html, cancellationToken: cancellationToken);
            }
            catch (Exception exception)
            {
                if (exception.IsIgnorable())
                {
                    logger.LogDebug(exception, "Failed to send Salat Time notification to ChatId: {ChatId}", request.ChatId);
                    break;
                }
                else
                {
                    logger.LogError(exception, "Error occured send Salat Time notification to ChatId: {ChatId}", request.ChatId);
                }
            }
        }

        return true;
    }
}
