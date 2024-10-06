using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.Application.Handlers.RestApis.Webhook;

public class SendWebhookMessageRequest : IRequest<object>
{
    public string targetId { get; set; }
    public string Event { get; set; }
    public string TransactionId { get; set; }
    public WebhookSource WebhookSource { get; set; }
    public string RawBody { get; set; }
    public string FormattedHtml { get; set; }
    public bool IsDebug { get; set; }
    public string RawHeaders { get; set; }
}

public class SendWebhookMessageRequestHandler(
    ILogger<SendWebhookMessageRequestHandler> logger,
    DataFacade dataFacade
) : IRequestHandler<SendWebhookMessageRequest, object>
{
    public async Task<object> Handle(SendWebhookMessageRequest request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var webhookChat = await dataFacade.ChatSetting.GetWebhookRouteById(request.targetId);
        var botSetting = await dataFacade.AppSetting.GetBotMain();
        var botClient = new TelegramBotClient(botSetting.Token);

        Message sentMessage = new();

        var lastMessageId = await dataFacade.ChatSetting.LastWebhookMessageBetterEdit(webhookChat.ChatId, request.WebhookSource, request.Event);

        try
        {
            if (lastMessageId != 0)
                sentMessage = await botClient.EditMessageTextAsync(
                    webhookChat.ChatId,
                    lastMessageId,
                    request.FormattedHtml,
                    ParseMode.Html,
                    disableWebPagePreview: true,
                    cancellationToken: cancellationToken);
            else
                sentMessage = await botClient.SendTextMessageAsync(
                    webhookChat.ChatId,
                    request.FormattedHtml,
                    webhookChat.MessageThreadId,
                    ParseMode.Html,
                    disableWebPagePreview: true,
                    cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Trying send GitHub Webhook without thread to ChatId: {ChatId}", webhookChat.ChatId);
            if (exception.Message.Contains("thread not found"))
            {
                sentMessage = await botClient.SendTextMessageAsync(
                    webhookChat.ChatId,
                    request.FormattedHtml,
                    parseMode: ParseMode.Html,
                    disableWebPagePreview: true,
                    cancellationToken: cancellationToken);
            }
            else
            {
                logger.LogError(exception, "Fail when sending webhook Message to ChatId: {ChatId}, ThreadId: {ThreadId}",
                    webhookChat.ChatId, webhookChat.MessageThreadId);

                // return response.BadRequest(exception.Message);
                return 0;
            }
        }

        dataFacade.MongoDb.WebhookHistory.Add(new WebhookHistoryEntity {
            RouteId = webhookChat.RouteId,
            TransactionId = request.TransactionId,
            CreatedDate = default,
            UpdatedDate = default,
            ChatId = webhookChat.ChatId,
            MessageId = sentMessage.MessageId,
            MessageThreadId = 0,
            WebhookSource = WebhookSource.GitHub,
            Elapsed = stopwatch.Elapsed,
            Payload = request.IsDebug ?
                request.RawBody :
                string.Empty,
            Header = request.IsDebug ?
                request.RawHeaders :
                default,
            EventName = request.Event,
            Status = (int)EventStatus.Complete
        });

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        var chatActivity = lastMessageId == 0 ?
            ChatActivityType.BotSendWebHook :
            ChatActivityType.BotEditWebHook;

        dataFacade.MongoDb.ChatActivity.Add(new ChatActivityEntity {
            ActivityType = chatActivity,
            ActivityTypeName = chatActivity.ToString(),
            ChatId = webhookChat.ChatId,
            UserId = sentMessage.From.Id,
            Chat = sentMessage.Chat,
            User = sentMessage.From,
            Status = (int)EventStatus.Complete,
            TransactionId = request.TransactionId,
            MessageId = sentMessage.MessageId
        });

        stopwatch.Stop();

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        return 1;
    }
}