using ZiziBot.Application.Common.Constants;
using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.Presentation.Bots.Telegram.Controllers;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class PingController(
    MediatorService mediatorService
) : BotCommandController
{
    [Command("ping")]
    [TextCommand("ping")]
    public async Task Ping(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new PingBotRequestModel()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            DeleteAfter = TimeSpan.FromMinutes(1),
            ReplyMessage = true,
            CleanupTargets =
            [
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            ]
        });
    }

    [Callback(CallbackConst.PING)]
    public async Task PingCallback(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new PingCallbackBotRequestModel()
        {
            BotToken = context.BotToken,
            CallbackQuery = context.CallbackQuery,
            ExecutionStrategy = ExecutionStrategy.Hangfire
        });
    }

    [DefaultCommand]
    public async Task Default(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new DefaultBotRequestModel()
        {
            BotToken = context.BotToken,
            Message = context.Message
        });
    }
}

