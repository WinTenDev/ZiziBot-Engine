using Allowed.Telegram.Bot.Attributes;
using Allowed.Telegram.Bot.Controllers;
using Allowed.Telegram.Bot.Models;

namespace ZiziBot.Allowed.TelegramBot.Controllers;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class PingController(MediatorService mediatorService) : CommandController
{
    [Command("ping")]
    [TextCommand("ping")]
    public async Task Ping(MessageData data)
    {
        await mediatorService.EnqueueAsync(new PingBotRequestModel() {
                BotToken = data.Options.Token,
                Message = data.Message,
                DeleteAfter = TimeSpan.FromMinutes(1),
                ReplyMessage = true,
                CleanupTargets = new[] {
                    CleanupTarget.FromBot,
                    CleanupTarget.FromSender
                }
            }
        );
    }

    [CallbackQuery(CallbackConst.BOT)]
    public async Task PingCallback(CallbackQueryData data, PingCallbackQueryModel model)
    {
        await mediatorService.EnqueueAsync(new PingCallbackBotRequestModel() {
                BotToken = data.Options.Token,
                CallbackQuery = data.CallbackQuery,
                ExecutionStrategy = ExecutionStrategy.Hangfire
            }
        );
    }

    [DefaultCommand]
    [TextCommand()]
    public async Task Default(MessageData data)
    {
        await mediatorService.EnqueueAsync(new DefaultBotRequestModel() {
                BotToken = data.Options.Token,
                Message = data.Message
            }
        );
    }
}