using MongoFramework.Linq;

public class DisableWordFilterRequest : BotRequestBase
{
    public string? Word { get; set; }
}

public class DisableWordFilterHandler(
    DataFacade dataFacade,
    ServiceFacade serviceFacade
) : IBotRequestHandler<DisableWordFilterRequest>
{
    public async Task<BotResponseBase> Handle(DisableWordFilterRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        if (request.Word == null)
        {
            return await serviceFacade.TelegramService.SendMessageAsync("Apa Word yang ingin dimatikan?");
        }

        var wordFilter = await dataFacade.MongoDb.WordFilter
            .Where(x => x.Word == request.Word)
            .Where(x => x.Status == (int)EventStatus.Complete)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (wordFilter == null)
        {
            return await serviceFacade.TelegramService.SendMessageAsync("Word tidak ditemukan");
        }

        wordFilter.Status = (int)EventStatus.Inactive;

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);
        await dataFacade.WordFilter.GetAllAsync(true);

        return await serviceFacade.TelegramService.SendMessageAsync("Word berhasil dimatikan");
    }
}