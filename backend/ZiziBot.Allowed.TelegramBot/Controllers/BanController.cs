using Allowed.Telegram.Bot.Attributes;
using Allowed.Telegram.Bot.Controllers;
using Allowed.Telegram.Bot.Models;

namespace ZiziBot.Allowed.TelegramBot.Controllers;

[BotName("Main")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class BanController(MediatorService mediatorService) : CommandController
{
    [Command("gban")]
    public async Task AddGlobalBan(MessageData data)
    {
        await mediatorService.EnqueueAsync(new AddBanBotRequest()
        {
            BotToken = data.Options.Token,
            Message = data.Message,
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