using Allowed.Telegram.Bot.Attributes;
using Allowed.Telegram.Bot.Controllers;
using Allowed.Telegram.Bot.Models;
using ZiziBot.Application.Handlers.Telegram.WordFilter;

namespace ZiziBot.Allowed.TelegramBot.Controllers;

[BotName("Main")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class WordFilterController(MediatorService mediatorService) : CommandController
{
    [Command("awf")]
    public async Task AddBadWordCommand(MessageData data)
    {
        await mediatorService.EnqueueAsync(new AddWordFilterRequest() {
            BotToken = data.Options.Token,
            Message = data.Message,
            ReplyMessage = true,
            MinimumRole = RoleLevel.Sudo,
            Word = data.Message.Text.GetCommandParamAt<string>(1),
            CleanupTargets = new[] {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }

    [Command("dwf")]
    public async Task DisableBadWordCommand(MessageData data)
    {
        await mediatorService.EnqueueAsync(new DisableWordFilterRequest() {
            BotToken = data.Options.Token,
            Message = data.Message,
            ReplyMessage = true,
            MinimumRole = RoleLevel.Sudo,
            Word = data.Message.Text.GetCommandParamAt<string>(1),
            CleanupTargets = new[] {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }
}