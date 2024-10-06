using Allowed.Telegram.Bot.Attributes;
using Allowed.Telegram.Bot.Controllers;
using Allowed.Telegram.Bot.Models;

namespace ZiziBot.Allowed.TelegramBot.Controllers;

[BotName("Main")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class CityController(MediatorService mediator) : CommandController
{
    [Command("ac")]
    [Command("addcity")]
    [Command("add_city")]
    public async Task AddCity(MessageData data)
    {
        await mediator.EnqueueAsync(new AddCityBotRequest()
        {
            BotToken = data.Options.Token,
            Message = data.Message,
            ReplyMessage = true,
            CityName = data.Params,
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }

    [Command("city")]
    [Command("lc")]
    public async Task GetCity(MessageData data)
    {
        await mediator.EnqueueAsync(new GetCityListBotRequest()
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

    [Command("salat")]
    [Command("shalat")]
    [Command("sholat")]
    public async Task GetShalatTime(MessageData data)
    {
        await mediator.EnqueueAsync(new GetShalatTimeBotRequest()
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
}