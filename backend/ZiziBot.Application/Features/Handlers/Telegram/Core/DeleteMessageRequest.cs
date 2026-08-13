using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace ZiziBot.Application.Features.Handlers.Telegram.Core;

public class DeleteMessageBotRequestModel : BotRequestBase
{
    public int MessageId { get; set; }
}

[UsedImplicitly]
public class DeleteMessageRequestHandler(
    ILogger<DeleteMessageRequestHandler> logger,
    ServiceFacade serviceFacade
)
    : IBotRequestHandler<DeleteMessageBotRequestModel>
{
    public async Task<BotResponseBase> Handle(DeleteMessageBotRequestModel request, CancellationToken cancellationToken)
    {
        var chatId = request.ChatIdentifier;

        serviceFacade.TelegramService.SetupResponse(request);

        if (chatId == 0)
            return serviceFacade.TelegramService.Complete();

        try
        {
            logger.LogDebug("Deleting message {MessageId} from chat {ChatId}", request.MessageId, chatId);
            await serviceFacade.TelegramService.Bot.DeleteMessage(chatId, request.MessageId, cancellationToken: cancellationToken);

            logger.LogInformation("Message {MessageId} deleted from chat {ChatId}", request.MessageId, chatId);
        }
        catch (Exception exception)
        {
            if (exception.IsIgnorable())
            {
                logger.LogWarning("Message {MessageId} could not be deleted from chat {ChatId}", request.MessageId, chatId);
            }
            else
            {
                logger.LogError(exception, "Message {MessageId} could not be deleted from chat {ChatId}", request.MessageId, chatId);
                throw;
            }
        }

        return serviceFacade.TelegramService.Complete();
    }
}