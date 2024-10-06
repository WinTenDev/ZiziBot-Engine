using Allowed.Telegram.Bot.Attributes;
using Allowed.Telegram.Bot.Controllers;
using Allowed.Telegram.Bot.Models;

namespace ZiziBot.Allowed.TelegramBot.Controllers;

[BotName("Main")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SettingsController(MediatorService mediatorService) : CommandController
{
    [Command("console")]
    [Command("settings")]
    public async Task GetSettingPanel(MessageData data)
    {
        await mediatorService.EnqueueAsync(new PrepareConsoleBotRequest()
        {
            BotToken = data.Options.Token,
            Message = data.Message,
            ReplyMessage = true,
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }

    [Command("wb")]
    [Command("wd")]
    [Command("wt")]
    public async Task SaveWelcome(MessageData data)
    {
        await mediatorService.EnqueueAsync(new SaveWelcomeMessageRequest()
        {
            BotToken = data.Options.Token,
            Message = data.Message,
            ReplyMessage = true,
            MinimumRole = RoleLevel.ChatAdmin,
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }

    [Command("wm")]
    public async Task ShowWelcome(MessageData data)
    {
        await mediatorService.EnqueueAsync(new ShowWelcomeMessageRequest()
        {
            BotToken = data.Options.Token,
            Message = data.Message,
            ReplyMessage = true,
            MinimumRole = RoleLevel.ChatAdmin,
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }
}