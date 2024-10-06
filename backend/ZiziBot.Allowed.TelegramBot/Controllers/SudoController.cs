using Allowed.Telegram.Bot.Attributes;
using Allowed.Telegram.Bot.Controllers;
using Allowed.Telegram.Bot.Models;

namespace ZiziBot.Allowed.TelegramBot.Controllers;

[BotName("Main")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SudoController(MediatorService mediatorService) : CommandController
{
    [Command("addsudo")]
    public async Task AddSudo(MessageData data)
    {
        await mediatorService.EnqueueAsync(new AddSudoBotRequestModel()
        {
            BotToken = data.Options.Token,
            Message = data.Message,
            ReplyMessage = true,
            CustomUserId = data.Message.Text.GetCommandParamAt<long>(1),
            DeleteAfter = TimeSpan.FromMinutes(1),
            ReplyToMessageId = data.Message.MessageId,
        });
    }
}