namespace ZiziBot.Application.Handlers.Telegram.WordFilter;

public class AddWordFilterRequest : BotRequestBase
{
    public string? Word { get; set; }
}

public class AddWordFilterHandler(
    ServiceFacade serviceFacade,
    DataFacade dataFacade
) : IBotRequestHandler<AddWordFilterRequest>
{
    public async Task<BotResponseBase> Handle(AddWordFilterRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        if (request.Word.IsNullOrEmpty())
        {
            return await serviceFacade.TelegramService.SendMessageAsync("Apa kata yang ingin ditambahkan?");
        }

        List<PipelineResultAction> action = new();

        var cmdParam = request.Params?.Skip(1).ToList();
        if (cmdParam.NotEmpty())
        {
            if (cmdParam.Contains("-d"))
                action.Add(PipelineResultAction.Delete);

            if (cmdParam.Contains("-w"))
                action.Add(PipelineResultAction.Warn);

            if (cmdParam.Contains("-m"))
                action.Add(PipelineResultAction.Mute);

            if (cmdParam.Contains("-k"))
                action.Add(PipelineResultAction.Kick);
        }

        await dataFacade.WordFilter.SaveAsync(new WordFilterDto() {
            ChatId = request.ChatIdentifier,
            UserId = request.UserId,
            Word = request.Word,
            Action = action.ToArray(),
            TransactionId = request.TransactionId
        });

        return await serviceFacade.TelegramService.SendMessageAsync("Kata berhasil disimpan");
    }
}