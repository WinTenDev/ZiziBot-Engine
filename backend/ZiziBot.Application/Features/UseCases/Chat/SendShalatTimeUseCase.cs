using System.ComponentModel;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Features.UseCases.Chat;

public class SendShalatTimeUseCase(
    ILogger<SendShalatTimeUseCase> logger,
    DataFacade dataFacade,
    ServiceFacade serviceFacade,
    SendMessageUseCase sendMessageUseCase
)
{
    [DisplayName("ShalatTIme {0}")]
    [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete, Attempts = 1)]
    public async Task<bool> Handle(long chatId)
    {
        var botMain = await dataFacade.Bot.GetBotMain();

        const string defaultMessage = "Telah masuk waktu <b>{Shalat}</b> untuk wilayah <b>{City}</b> dan sekitarnya.";

        var cityList = await dataFacade.ChatSetting.GetShalatCity(chatId);

        if (cityList.IsEmpty())
        {
            logger.LogInformation("City list is empty for ChatId: {ChatId}", chatId);
            return false;
        }

        logger.LogDebug("Found about {Count} city(es) for ChatId: {ChatId}", cityList.Count, chatId);

        foreach (var cityEntity in cityList)
        {
            try
            {
                logger.LogDebug("Checking Shalat time for city: '{CityName}'", cityEntity.CityName);
                var currentShalat = await serviceFacade.FathimahRestService.GetCurrentShalatTime(cityEntity.CityId);

                var currentDate = DateTime.UtcNow.AddHours(Env.DEFAULT_TIMEZONE);

                if (currentShalat.IsNullOrDefault())
                {
                    logger.LogDebug("No match Shalat time for city: '{CityName}' at '{CurrentTime}'", cityEntity.CityName, currentDate.ToString("HH:mm"));

                    continue;
                }

                var messageText = defaultMessage.ResolveVariable(new List<(string placeholder, string value)> {
                    ("Shalat", currentShalat?.Key ?? string.Empty),
                    ("Date", currentDate.ToString("yyyy-MM-dd HH:mm:ss")),
                    ("City", cityEntity.CityName)
                });

                await sendMessageUseCase.Handle(new SendMessageRequest() {
                    BotToken = botMain.Token,
                    ChatId = chatId,
                    Text = messageText,
                    DeleteAfter = TimeSpan.FromDays(7)
                });
            }
            catch (Exception exception)
            {
                if (exception.IsIgnorable())
                {
                    logger.LogWarning(exception, "Fail to send Salat Time notification to ChatId: {ChatId}", chatId);
                    break;
                }
                else
                {
                    logger.LogError(exception, "Error occurred to send Salat Time notification to ChatId: {ChatId}", chatId);
                }
            }
        }

        return true;
    }
}