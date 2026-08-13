using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Features.Handlers.Telegram.Chat;

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
            .Where(entity => entity.Status == EventStatus.Complete)
            .FirstOrDefaultAsync();

        if (channelMap == null)
        {
            dataFacade.MongoDb.ChannelMap.Add(new ChannelMapEntity() {
                ChannelId = request.ChannelId,
                ThreadId = request.MessageThreadId,
                ChatId = request.ChatIdentifier,
                Status = EventStatus.Complete
            });

            await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);
        }

        await serviceFacade.TelegramService.EditMessageText("Berhasil menautkan Kanal.");

        return serviceFacade.TelegramService.Complete();
    }
}
