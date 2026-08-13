using ZiziBot.Application.Features.Handlers.Telegram.Rss;
using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.Presentation.Bots.Telegram.Controllers;

// [BotName("Main")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class RssController(
    MediatorService mediatorService
) : BotCommandController
{
    [Command("rss")]
    public async Task AddRss(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new AddRssRequest()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            MinimumRole = RoleLevel.ChatAdminOrPrivate,
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }

    [Command("drss")]
    public async Task DisableRss(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new DisableRssRequest()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            MinimumRole = RoleLevel.ChatAdminOrPrivate,
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }
}

