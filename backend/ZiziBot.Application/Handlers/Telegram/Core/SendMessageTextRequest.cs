using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace ZiziBot.Application.Handlers.Telegram.Core;

public class SendMessageTextBotRequestModel : BotRequestBase
{ }

[UsedImplicitly]
[Obsolete("Send Message via ResponseBase")]
public class SendMessageTextRequestHandler(
    ILogger<SendMessageTextRequestHandler> logger,
    DataFacade dataFacade,
    ServiceFacade serviceFacade
)
    : IRequestHandler<SendMessageTextBotRequestModel, BotResponseBase>
{
    public async Task<BotResponseBase> Handle(SendMessageTextBotRequestModel request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        logger.LogDebug("Sending message to chat {ChatId}", request.ChatId);
        var sentMessage = await serviceFacade.TelegramService.Bot.SendTextMessageAsync(
            chatId: request.ChatId,
            text: request.Text,
            replyToMessageId: request.ReplyToMessageId,
            cancellationToken: cancellationToken
        );

        logger.LogInformation("Message sent to chat {ChatId}", request.ChatId);

        if (request.DeleteAfter.Ticks <= 0)
            return serviceFacade.TelegramService.Complete();

        logger.LogDebug("Deleting message {MessageId} in {DeleteAfter} seconds", sentMessage.MessageId, request.DeleteAfter.TotalSeconds);
        serviceFacade.MediatorService.Schedule(
            new DeleteMessageBotRequestModel() {
                BotToken = request.BotToken,
                Message = request.Message,
                MessageId = sentMessage.MessageId,
                ExecutionStrategy = request.ExecutionStrategy,
                DeleteAfter = request.DeleteAfter
            }
        );

        logger.LogInformation("Message {MessageId} scheduled for deletion in {DeleteAfter} seconds", sentMessage.MessageId,
            request.DeleteAfter.TotalSeconds);

        return serviceFacade.TelegramService.Complete();
    }
}