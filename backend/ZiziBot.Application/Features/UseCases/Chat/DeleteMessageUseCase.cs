using Hangfire;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using ZiziBot.Application.Features.Hangfire;

namespace ZiziBot.Application.Features.UseCases.Chat;

public class DeleteMessageBotRequest
{
    public string BotToken { get; set; }
    public int MessageId { get; set; }
    public long ChatId { get; set; }
}

public class DeleteMessageUseCase(
    ILogger<DeleteMessageUseCase> logger,
    ServiceFacade serviceFacade
)

{
    [Queue(CronJobKey.Queue_Telegram)]
    public async Task Handle(DeleteMessageBotRequest request)
    {
        try
        {
            logger.LogDebug("Deleting message {MessageId} from chat {ChatId}", request.MessageId, request.ChatId);
            var botClient = new TelegramBotClient(request.BotToken);
            await botClient.DeleteMessage(request.ChatId, request.MessageId);

            logger.LogInformation("Message {MessageId} deleted from chat {ChatId}", request.MessageId, request.ChatId);

            HangfireUtil.Enqueue<CreateChatActivityUseCase>(x => x.Handle(new CreateChatActivityRequest {
                ActivityType = ChatActivityType.BotSendMessage,
                MessageId = request.MessageId,
                TransactionId = Guid.NewGuid().ToString()
            }));
        }
        catch (Exception exception)
        {
            if (exception.IsIgnorable())
            {
                logger.LogWarning("Message {MessageId} could not be deleted from chat {ChatId}", request.MessageId, request.ChatId);
            }
            else
            {
                logger.LogError(exception, "Message {MessageId} could not be deleted from chat {ChatId}", request.MessageId, request.ChatId);
                throw;
            }
        }
    }
}