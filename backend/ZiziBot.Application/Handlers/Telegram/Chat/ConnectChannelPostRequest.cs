using Microsoft.Extensions.Logging;
using MongoFramework.Linq;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.Application.Handlers.Telegram.Chat;

public class ConnectChannelPostRequest : BotRequestBase
{
    public long ChannelId { get; set; }
}

public class ConnectChannelPostHandler(
    ILogger<ConnectChannelPostHandler> logger,
    DataFacade dataFacade,
    ServiceFacade serviceFacade
) : IBotRequestHandler<ConnectChannelPostRequest>
{
    private readonly ILogger<ConnectChannelPostHandler> _logger = logger;

    public async Task<BotResponseBase> Handle(ConnectChannelPostRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        await serviceFacade.TelegramService.SendMessageText("Sedang menautkan Kanal..");

        if (request.ChannelId == 0)
        {
            return await serviceFacade.TelegramService.EditMessageText("Spesifikasikan ChannelId yang ingin ditautkan" +
                                                                       "\nContoh: <code>/fch -1001139107957</code>");
        }

        var channel = await serviceFacade.TelegramService.GetChatAsync(request.ChannelId);
        if (channel is null)
        {
            return await serviceFacade.TelegramService.EditMessageText("ChannelId tidak ditemukan. " +
                                                                       "\nPastikan Bot sudah ditambahkan ke Channel tersebut");
        }

        var channelMap = await dataFacade.MongoDb.ChannelMap.AsNoTracking()
            .Where(entity => entity.ChannelId == request.ChannelId)
            .Where(entity => entity.ChatId == request.ChatIdentifier)
            .Where(entity => entity.ThreadId == request.MessageThreadId)
            .Where(entity => entity.Status == (int)EventStatus.Complete)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (channelMap == null)
        {
            dataFacade.MongoDb.ChannelMap.Add(new ChannelMapEntity() {
                ChannelId = request.ChannelId,
                ThreadId = request.MessageThreadId,
                ChatId = request.ChatIdentifier,
                Status = (int)EventStatus.Complete
            });

            await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);
        }

        await serviceFacade.TelegramService.EditMessageText("Berhasil menautkan Kanal.");

        return serviceFacade.TelegramService.Complete();
    }
}