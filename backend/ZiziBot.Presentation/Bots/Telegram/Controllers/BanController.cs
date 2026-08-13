using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.Presentation.Bots.Telegram.Controllers;

// [BotName("Main")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class BanController(
    MediatorService mediatorService
) : BotCommandController
{
    [Command("gban")]
    public async Task AddGlobalBan(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new AddBanBotRequest()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            ReplyMessage = true,
            DeleteAfter = TimeSpan.FromHours(1),
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }
}

