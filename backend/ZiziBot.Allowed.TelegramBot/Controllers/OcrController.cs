using Allowed.Telegram.Bot.Attributes;
using Allowed.Telegram.Bot.Controllers;
using Allowed.Telegram.Bot.Models;

namespace ZiziBot.Allowed.TelegramBot.Controllers;

[BotName("Main")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class OcrController(MediatorService mediatorService) : CommandController
{
    [Command("ocr")]
    public async Task Ocr(MessageData data)
    {
        await mediatorService.EnqueueAsync(new OcrBotRequest()
            {
                BotToken = data.Options.Token,
                Message = data.Message,
                ReplyMessage = true,
                DeleteAfter = TimeSpan.FromHours(1)
            }
        );
    }
}