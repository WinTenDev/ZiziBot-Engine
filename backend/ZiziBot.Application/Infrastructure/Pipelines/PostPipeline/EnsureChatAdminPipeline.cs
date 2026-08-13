using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Infrastructure.Pipelines.PostPipeline;

public class EnsureChatAdminPipeline<TRequest, TResponse>(
    ILogger<EnsureChatAdminPipeline<TRequest, TResponse>> logger,
    DataFacade dataFacade,
    ServiceFacade serviceFacade
) : ITelegramPostProcessPipeline<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task ProcessAsync(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        if (request is not BotRequestBase botRequest)
            return;

        if (botRequest.ChatType == ChatType.Private ||
            botRequest.IsChannel ||
            botRequest.Source != ResponseSource.Bot ||
            botRequest.ChatId == 0 ||
            botRequest.InlineQuery != null)
            return;

        serviceFacade.TelegramService.SetupResponse(botRequest);

        var listChatAdmin = await dataFacade.MongoDb.ChatAdmin
            .Where(entity => entity.ChatId == botRequest.ChatIdentifier)
            .Where(entity => entity.Status == EventStatus.Complete)
            .ToListAsync(cancellationToken);

        dataFacade.MongoDb.ChatAdmin.RemoveRange(listChatAdmin);
        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        var chatAdministrators = await serviceFacade.TelegramService.GetChatAdministrator();
        logger.LogDebug("Admin count in ChatId: {ChatId} found {ChatAdministrators} item(s)", botRequest.ChatId, chatAdministrators.Count);

        var chatAdminEntities = chatAdministrators.Select(admin => new ChatAdminEntity {
            ChatId = botRequest.ChatIdentifier,
            UserId = admin.User.Id,
            Role = admin.Status,
            Status = EventStatus.Complete
        }).ToList();

        dataFacade.MongoDb.ChatAdmin.AddRange(chatAdminEntities);
        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);
    }
}
