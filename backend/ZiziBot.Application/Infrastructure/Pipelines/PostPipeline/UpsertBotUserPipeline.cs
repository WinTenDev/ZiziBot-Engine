using Microsoft.Extensions.Logging;
using Telegram.Bot;
using ZiziBot.Application.Common.Dtos;

namespace ZiziBot.Application.Infrastructure.Pipelines.PostPipeline;

public class UpsertBotUserPipeline<TRequest, TResponse>(
    ILogger<UpsertBotUserPipeline<TRequest, TResponse>> logger,
    ServiceFacade serviceFacade,
    DataFacade dataFacade
) : ITelegramPostProcessPipeline<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task ProcessAsync(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        if (request is not BotRequestBase botRequest)
            return;

        if (botRequest.Source != ResponseSource.Bot ||
            botRequest.IsChannel)
            return;

        logger.LogDebug("Updating User information for UserId: {UserId}", botRequest.UserId);

        botRequest.DeleteAfter = TimeSpan.FromDays(1);

        serviceFacade.TelegramService.SetupResponse(botRequest);

        var userProfilePhotos = await serviceFacade.TelegramService.Bot.GetUserProfilePhotos(botRequest.UserId);
        var profilePhotoId = userProfilePhotos.Photos.FirstOrDefault()?.OrderByDescending(size => size.FileSize).FirstOrDefault()?.FileId ?? string.Empty;

        var userActivity = await dataFacade.ChatSetting.SaveUserActivity(new BotUserDto() {
            User = botRequest.User,
            FirstName = botRequest.FirstName,
            LastName = botRequest.LastName,
            LanguageCode = botRequest.UserLanguageCode,
            ProfilePhotoId = profilePhotoId,
            Username = botRequest.Username,
            UserId = botRequest.UserId,
            TransactionId = botRequest.TransactionId
        });

        if (!string.IsNullOrEmpty(userActivity))
        {
            await serviceFacade.TelegramService.SendMessageText(userActivity);
        }

        logger.LogInformation("User information for UserId: {UserId} has been updated", botRequest.UserId);
    }
}