using Allowed.Telegram.Bot.Attributes;
using Allowed.Telegram.Bot.Controllers;
using Allowed.Telegram.Bot.Models;

namespace ZiziBot.Allowed.TelegramBot.Controllers;

[BotName("Main")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SudoController : CommandController
{
    private readonly MediatorService _mediatorService;

    public SudoController(MediatorService mediatorService)
    {
        _mediatorService = mediatorService;
    }

    [Command("addsudo")]
    public async Task AddSudo(MessageData data)
    {
        await _mediatorService.EnqueueAsync(
            new AddSudoRequestModel()
        {
            BotToken = data.Options.Token,
            Message = data.Message,
            DeleteAfter = TimeSpan.FromMinutes(1),
            ReplyToMessageId = data.Message.MessageId,
        });
    }
}