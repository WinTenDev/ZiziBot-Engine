using ZiziBot.Application.Features.Handlers.Telegram.Spelling;
using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.Presentation.Bots.Telegram.Controllers;

[BotName("Main")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SpellingController(MediatorService mediatorService) : BotCommandController
{
    [Command("spell")]
    public async Task CreateSpelling(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new ManageSpellingBotRequest()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            ReplyMessage = true,
            MinimumRole = RoleLevel.Sudo,
            CleanupTargets = [CleanupTarget.FromBot, CleanupTarget.FromSender]
        });
    }
}

