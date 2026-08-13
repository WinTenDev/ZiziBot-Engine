using Hangfire;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ZiziBot.Application.Features.Hangfire;

namespace ZiziBot.Application.Features.UseCases.Chat;

public class SendMessageRequest
{
    public string BotToken { get; set; }
    public long ChatId { get; set; }
    public int? ThreadId { get; set; }
    public string Text { get; set; }
    public bool DisableWebPagePreview { get; set; }
    public int ReplyToMessageId { get; set; }
    public TimeSpan DeleteAfter { get; set; }
}

public class SendMessageUseCase(
    ILogger<SendMessageUseCase> logger,
    DataFacade dataFacade,
    ServiceFacade serviceFacade,
    CreateChatActivityUseCase createChatActivityUseCase
)
{
    [Queue(CronJobKey.Queue_Telegram)]
    public async Task Handle(SendMessageRequest request)
    {
        try
        {
            logger.LogDebug("Sending a message to chat {ChatId}", request.ChatId);
            var botClient = new TelegramBotClient(request.BotToken);

            var sentMessage = await botClient.SendMessage(
                chatId: request.ChatId,
                text: request.Text,
                messageThreadId: request.ThreadId,
                replyParameters: new ReplyParameters() {
                    AllowSendingWithoutReply = true,
                    MessageId = request.ReplyToMessageId
                },
                parseMode: ParseMode.Html,
                linkPreviewOptions: new LinkPreviewOptions() {
                    IsDisabled = request.DisableWebPagePreview
                }
            );

            logger.LogInformation("Message sent to chat {ChatId}, MessageId: {MessageId}", request.ChatId, sentMessage.MessageId);

            HangfireUtil.Enqueue<CreateChatActivityUseCase>(x => x.Handle(new CreateChatActivityRequest {
                ActivityType = ChatActivityType.BotSendMessage,
                ChatId = request.ChatId,
                ThreadId = request.ThreadId,
                MessageId = sentMessage.MessageId,
                TransactionId = Guid.NewGuid().ToString()
            }));

            if (request.DeleteAfter.Ticks <= 0)
                return;

            logger.LogDebug("Deleting message {MessageId} in {DeleteAfter} seconds", sentMessage.MessageId, request.DeleteAfter.TotalSeconds);
            HangfireUtil.Schedule<DeleteMessageUseCase>(x => x.Handle(new DeleteMessageBotRequest {
                BotToken = request.BotToken,
                MessageId = sentMessage.MessageId,
                ChatId = request.ChatId
            }), request.DeleteAfter);

            logger.LogInformation("Message {MessageId} scheduled for deletion in {DeleteAfter} seconds", sentMessage.MessageId, request.DeleteAfter.TotalSeconds);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to send message to chat {ChatId}", request.ChatId);

            if (!e.IsIgnorable())
                throw;
        }
    }
}