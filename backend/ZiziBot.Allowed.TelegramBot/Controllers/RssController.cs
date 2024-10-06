using Allowed.Telegram.Bot.Attributes;
using Allowed.Telegram.Bot.Controllers;
using Allowed.Telegram.Bot.Models;
using ZiziBot.Application.Handlers.Telegram.Rss;

namespace ZiziBot.Allowed.TelegramBot.Controllers;

[BotName("Main")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class RssController(MediatorService mediatorService) : CommandController
{
    [Command("rss")]
    public async Task AddRss(MessageData data)
    {
        await mediatorService.EnqueueAsync(new AddRssRequest()
        {
            BotToken = data.Options.Token,
            Message = data.Message,
            MinimumRole = RoleLevel.ChatAdminOrPrivate,
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }
}