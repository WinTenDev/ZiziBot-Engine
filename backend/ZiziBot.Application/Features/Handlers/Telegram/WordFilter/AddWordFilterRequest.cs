using ZiziBot.Application.Common.Dtos;

namespace ZiziBot.Application.Features.Handlers.Telegram.WordFilter;

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

        var wordFilterDto = new WordFilterDto() {
            ChatId = request.ChatIdentifier,
            UserId = request.UserId,
            Word = request.Word,
            TransactionId = request.TransactionId
        };

        var cmdParam = request.Params?.Skip(1).FirstOrDefault();
        if (cmdParam?.StartsWith('-') ?? false)
        {
            if (cmdParam.Contains('d'))
                action.Add(PipelineResultAction.Delete);

            if (cmdParam.Contains('w'))
                action.Add(PipelineResultAction.Warn);

            if (cmdParam.Contains('m'))
                action.Add(PipelineResultAction.Mute);

            if (cmdParam.Contains('k'))
                action.Add(PipelineResultAction.Kick);

            wordFilterDto.IsRegex = cmdParam.Contains('r');
        }

        wordFilterDto.Action = action.ToArray();

        await dataFacade.WordFilter.SaveAsync(wordFilterDto);

        return await serviceFacade.TelegramService.SendMessageAsync("Kata berhasil disimpan");
    }
}