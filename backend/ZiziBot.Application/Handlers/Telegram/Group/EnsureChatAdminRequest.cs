using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.Application.Handlers.Telegram.Group;

public class EnsureChatAdminRequestHandler<TRequest, TResponse>(
    ILogger<EnsureChatAdminRequestHandler<TRequest, TResponse>> logger,
    DataFacade dataFacade,
    ServiceFacade serviceFacade
)
    : IRequestPostProcessor<TRequest, TResponse>
    where TRequest : BotRequestBase, IRequest<TResponse>
    where TResponse : BotResponseBase
{
    public async Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        if (request.ChatType == ChatType.Private ||
            request.IsChannel ||
            request.Source != ResponseSource.Bot ||
            request.InlineQuery != null)
            return;

        serviceFacade.TelegramService.SetupResponse(request);

        dataFacade.MongoDb.ChatAdmin
            .RemoveRange(
                entity =>
                    entity.ChatId == request.ChatIdentifier
            );

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        var chatAdministrators = await serviceFacade.TelegramService.GetChatAdministrator();
        logger.LogDebug("List of Administrator in ChatId: {ChatId} found {ChatAdministrators} item(s)", request.ChatId,
            chatAdministrators.Count);

        var chatAdminEntities = chatAdministrators
            .Select(
                x => new ChatAdminEntity() {
                    ChatId = request.ChatIdentifier,
                    UserId = x.User.Id,
                    Role = x.Status,
                    Status = (int)EventStatus.Complete
                }
            )
            .ToList();

        dataFacade.MongoDb.ChatAdmin.AddRange(chatAdminEntities);

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);
    }
}