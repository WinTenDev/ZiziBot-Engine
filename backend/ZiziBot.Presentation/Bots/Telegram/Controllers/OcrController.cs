using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.Presentation.Bots.Telegram.Controllers;

// [BotName("Main")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class OcrController(
    MediatorService mediatorService
) : BotCommandController
{
    [Command("ocr")]
    public async Task Ocr(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new OcrBotRequest()
            {
                BotToken = context.BotToken,
                Message = context.Message,
                ReplyMessage = true,
                DeleteAfter = TimeSpan.FromHours(1)
            }
        );
    }
}

