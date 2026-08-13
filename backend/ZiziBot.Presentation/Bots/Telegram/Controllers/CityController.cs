using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.Presentation.Bots.Telegram.Controllers;

// [BotName("Main")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class CityController(
    MediatorService mediator
) : BotCommandController
{
    [Command("ac")]
    [Command("addcity")]
    [Command("add_city")]
    public async Task AddCity(CommandContext context)
    {
        await mediator.EnqueueAsync(new AddCityBotRequest()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            ReplyMessage = true,
            CityName = context.CommandParam,
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }

    [Command("city")]
    [Command("lc")]
    public async Task GetCity(CommandContext context)
    {
        await mediator.EnqueueAsync(new GetCityListBotRequest()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            ReplyMessage = true,
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }

    [Command("salat")]
    [Command("shalat")]
    [Command("sholat")]
    public async Task GetShalatTime(CommandContext context)
    {
        await mediator.EnqueueAsync(new GetShalatTimeBotRequest()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            ReplyMessage = true,
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }
}

