using Microsoft.Extensions.Logging;
using MongoFramework.Linq;

namespace ZiziBot.Application.Handlers.Telegram.Chat;

public class CreateWebhookBotRequest : BotRequestBase
{
}

public class CreateWebhookHandler : IRequestHandler<CreateWebhookBotRequest, BotResponseBase>
{
    private readonly ILogger<CreateWebhookHandler> _logger;
    private readonly TelegramService _telegramService;
    private readonly ChatDbContext _chatDbContext;

    public CreateWebhookHandler(ILogger<CreateWebhookHandler> logger, TelegramService telegramService, ChatDbContext chatDbContext)
    {
        _logger = logger;
        _telegramService = telegramService;
        _chatDbContext = chatDbContext;
    }

    public async Task<BotResponseBase> Handle(CreateWebhookBotRequest request, CancellationToken cancellationToken)
    {
        _telegramService.SetupResponse(request);

        await _telegramService.SendMessageText("Sedang membuat webhook..");

        var webhookChat = await _chatDbContext.WebhookChat
            .Where(entity => entity.ChatId == request.ChatIdentifier)
            .Where(entity => entity.Status == (int)EventStatus.Complete)
            .Where(entity => entity.MessageThreadId == request.MessageThreadId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        var routeId = await StringUtil.GetNanoIdAsync();
        var webhookUrl = UrlConst.WEBHOOK_URL + routeId;
        var htmlMessage = HtmlMessage.Empty
            .Bold("Webhook").Br();

        if (webhookChat == null)
        {
            _chatDbContext.WebhookChat.Add(new WebhookChatEntity()
            {
                ChatId = request.ChatIdentifier,
                MessageThreadId = request.MessageThreadId,
                RouteId = routeId,
                Status = (int)EventStatus.Complete,
            });
        }
        else
        {
            webhookUrl = UrlConst.WEBHOOK_URL + webhookChat.RouteId;
        }

        htmlMessage.Code(webhookUrl);
        await _chatDbContext.SaveChangesAsync(cancellationToken);

        return await _telegramService.EditMessageText(htmlMessage.ToString());
    }
}