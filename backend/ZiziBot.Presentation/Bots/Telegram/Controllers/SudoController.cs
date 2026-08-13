using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.Presentation.Bots.Telegram.Controllers;

// [BotName("Main")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SudoController(
    MediatorService mediatorService
) : BotCommandController
{
    [Command("addsudo")]
    public async Task AddSudo(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new AddSudoBotRequestModel()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            ReplyMessage = true,
            CustomUserId = context.MessageText.GetCommandParamAt<long>(1),
            DeleteAfter = TimeSpan.FromMinutes(1),
            ReplyToMessageId = context.Message.MessageId,
        });
    }
}

