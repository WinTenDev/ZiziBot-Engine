using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.Presentation.Bots.Telegram.Controllers;

// [BotName("Main")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class MirrorController(
    MediatorService mediatorService
) : BotCommandController
{
    [Command("mp")]
    public async Task SaveMirrorPayment(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new SavePaymentBotRequestModel()
        {
            BotToken = context.BotToken,
            ReplyMessage = true,
            Message = context.Message,
            Payload = context.CommandParam.GetCommandParamAt<string>(0),
            ForUserId = context.CommandParam.GetCommandParamAt<long>(1),
            MinimumRole = RoleLevel.Sudo,
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }

    [Command("sp")]
    public async Task SubmitMirrorPayment(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new SubmitPaymentBotRequest()
        {
            BotToken = context.BotToken,
            ReplyMessage = true,
            Message = context.Message,
            Payload = context.CommandParam.GetCommandParamAt<string>(0),
            ForUserId = context.CommandParam.GetCommandParamAt<long>(1),
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }
}

