using MongoFramework.Linq;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.Application.Handlers.Telegram.Group;

public class SetAfkBotRequest : BotRequestBase
{
    public string? Reason { get; set; }
}

public class SetAfkRequestHandler(
    DataFacade dataFacade,
    ServiceFacade serviceFacade
)
    : IRequestHandler<SetAfkBotRequest, BotResponseBase>
{
    public async Task<BotResponseBase> Handle(SetAfkBotRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        var afkEntity = await dataFacade.MongoDb.Afk
            .FirstOrDefaultAsync(entity =>
                    entity.UserId == request.UserId &&
                    entity.Status == (int)EventStatus.Complete,
                cancellationToken: cancellationToken);

        if (afkEntity == null)
        {
            dataFacade.MongoDb.Afk.Add(new AfkEntity() {
                UserId = request.UserId,
                ChatId = request.ChatIdentifier,
                Reason = request.Reason,
                Status = (int)EventStatus.Complete
            });
        }
        else
        {
            afkEntity.Reason = request.Reason;
            afkEntity.TransactionId = Guid.NewGuid().ToString();
            afkEntity.Status = (int)EventStatus.Complete;
        }

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        var htmlMessage = HtmlMessage.Empty
            .User(request.User)
            .Text(" sedang AFK");

        if (request.Reason != null)
            htmlMessage.Br()
                .Bold("Alasan: ").Text(request.Reason);

        return await serviceFacade.TelegramService.SendMessageText(htmlMessage.ToString());
    }
}