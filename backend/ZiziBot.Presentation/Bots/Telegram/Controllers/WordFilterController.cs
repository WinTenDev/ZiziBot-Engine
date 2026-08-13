using ZiziBot.Application.Features.Handlers.Telegram.WordFilter;
using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.Presentation.Bots.Telegram.Controllers;

// [BotName("Main")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class WordFilterController(
    MediatorService mediatorService
) : BotCommandController
{
    [Command("awf")]
    public async Task AddBadWordCommand(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new AddWordFilterRequest()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            ReplyMessage = true,
            MinimumRole = RoleLevel.Sudo,
            Word = context.Message.Text.GetCommandParamAt<string>(1),
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }

    [Command("dwf")]
    public async Task DisableBadWordCommand(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new DisableWordFilterRequest()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            ReplyMessage = true,
            MinimumRole = RoleLevel.Sudo,
            Word = context.Message.Text.GetCommandParamAt<string>(1),
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }
}

