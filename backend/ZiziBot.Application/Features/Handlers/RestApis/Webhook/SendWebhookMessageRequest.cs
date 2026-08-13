using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ZiziBot.Application.Common.Types;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Features.Handlers.RestApis.Webhook;

public class SendWebhookMessageRequest : IAppCommand<object>, IRequest<object>
{
    public string TargetId { get; set; }
    public string Event { get; set; }
    public string TransactionId { get; set; }
    public WebhookSource WebhookSource { get; set; }
    public WebhookHeader? WebhookHeader { get; set; }
    public string RawBody { get; set; }
    public string FormattedHtml { get; set; }
    public bool IsDebug { get; set; }
    public string RawHeaders { get; set; }
}

public class SendWebhookMessageRequestHandler(
    ILogger<SendWebhookMessageRequestHandler> logger,
    DataFacade dataFacade
)
    : IRequestHandler<SendWebhookMessageRequest, object>
{
    public async Task<object> Handle(SendWebhookMessageRequest request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var webhookChat = await dataFacade.ChatSetting.GetWebhookRouteById(request.TargetId);
        var botSetting = await dataFacade.Bot.GetBotMain();
        var botClient = new TelegramBotClient(botSetting.Token);
        var messageText = request.FormattedHtml;

        if (webhookChat == null)
        {
            return false;
        }

        if (webhookChat.IsDebug == true)
        {
            messageText = "[DEBUG MODE]" +
                          "\n\n" + messageText;
        }

        Message sentMessage = new();

        var lastMessageId = await dataFacade.ChatSetting.LastWebhookMessageBetterEdit(webhookChat.ChatId, request.WebhookSource, request.Event);

        try
        {
            if (lastMessageId != 0)
            {
                sentMessage = await botClient.EditMessageText(
                    chatId: webhookChat.ChatId,
                    messageId: lastMessageId,
                    text: messageText,
                    parseMode: ParseMode.Html,
                    linkPreviewOptions: true,
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                sentMessage = await botClient.SendMessage(
                    chatId: webhookChat.ChatId,
                    text: messageText,
                    messageThreadId: webhookChat.MessageThreadId,
                    parseMode: ParseMode.Html,
                    linkPreviewOptions: true,
                    cancellationToken: cancellationToken
                );
            }
        }
        catch (Exception exception)
        {
            if (exception.Message.Contains("thread not found"))
            {
                logger.LogWarning("Error send message to ChatId: {ChatId}. Trying to send GitHub Webhook without thread. Error: {Error}", webhookChat.ChatId, exception.Message);
                sentMessage = await botClient.SendMessage(
                    chatId: webhookChat.ChatId,
                    text: messageText,
                    parseMode: ParseMode.Html,
                    linkPreviewOptions: true,
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                if (exception.IsIgnorable())
                {
                    return 0;
                }
                else
                {
                    logger.LogError("Fail when sending webhook Message to ChatId: {ChatId}, ThreadId: {ThreadId}. Error: {Error}",
                        webhookChat.ChatId, webhookChat.MessageThreadId, exception.Message);
                }
            }
        }

        dataFacade.MongoDb.WebhookHistory.Add(new WebhookHistoryEntity {
            RouteId = webhookChat.RouteId,
            TransactionId = request.TransactionId,
            ChatId = webhookChat.ChatId,
            MessageId = sentMessage.MessageId,
            MessageThreadId = webhookChat.MessageThreadId,
            WebhookSource = request.WebhookSource,
            WebhookSourceName = request.WebhookSource.ToString(),
            UserAgent = request.WebhookHeader?.UserAgent,
            Elapsed = stopwatch.Elapsed,
            Payload = request.IsDebug ? request.RawBody : string.Empty,
            Header = request.IsDebug ? request.RawHeaders : null,
            EventName = request.Event,
            Status = EventStatus.Complete,
            IsDebug = webhookChat.IsDebug
        });

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        var chatActivity = lastMessageId == 0 ? ChatActivityType.BotSendWebHook : ChatActivityType.BotEditWebHook;

        dataFacade.MongoDb.ChatActivity.Add(new ChatActivityEntity {
            ActivityType = chatActivity,
            ActivityTypeName = chatActivity.ToString(),
            ChatId = webhookChat.ChatId,
            UserId = sentMessage.From?.Id,
            Status = EventStatus.Complete,
            // TransactionId = request.TransactionId,
            MessageId = sentMessage.MessageId
        });

        stopwatch.Stop();

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        return 1;
    }
}
