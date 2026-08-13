using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using ZiziBot.Application.Common.Types;

namespace ZiziBot.Application.Features.Handlers.Telegram.Basic;

public class GetAboutBotRequest : BotRequestBase
{ }

public class GetAboutHandler(
    DataFacade dataFacade,
    ServiceFacade serviceFacade
)
    : IBotRequestHandler<GetAboutBotRequest>
{
    public async Task<BotResponseBase> Handle(GetAboutBotRequest request, CancellationToken cancellationToken)
    {
        var htmlMessage = HtmlMessage.Empty;
        serviceFacade.TelegramService.SetupResponse(request);

        var me = await serviceFacade.TelegramService.Bot.GetMe(cancellationToken);
        var botFullMention = me.GetFullMention();

        var config = await dataFacade.AppSetting.GetConfigSectionAsync<EngineConfig>();

        var versionNumber = VersionUtil.GetVersionNumber();

        if (config != null)
        {
            htmlMessage
                .Bold(config.ProductName).Bold($" Build {versionNumber.Build} (").Code(VersionUtil.GetVersion()).Bold(")").Br()
                .Bold("Build Date: ").Code(VersionUtil.GetBuildDate().ToString("u")).Br()
                .Text("by ").Text(config.Vendor).Br()
                .Br();
        }

        htmlMessage.Text("Adalah bot pendebug dan manajemen grup. Ditulis menggunakan .NET (C#). ").Br().Br();
        htmlMessage.Text("Silakan bergabung ke channel <b>TeknoLugu Dev</b> untuk mendapatkan informasi terbaru mengenai pembaruan. " +
                         "Untuk detail fitur pada perintah /start.").Br().Br();

        htmlMessage
            .Text(
                $"Untuk <b>{botFullMention}</b> agar lebih cepat dan tetap cepat dan terus peningkatan dan keandalan, silakan <b>Sawer</b>/<b>Trakteer</b> untuk biaya Server dan Kopi 🍵.")
            .Br().Br();

        htmlMessage.Text("Terima kasih kepada <b>Akmal Projext</b> yang telah memberikan kesempatan <b>Zizi Bot</b> pada kehidupan sebelumnya.");

        var replyMarkup = new InlineKeyboardMarkup(new[] {
            new[] {
                InlineKeyboardButton.WithUrl("🪟 TeknoLugu", "https://t.me/TeknoLugu"),
                InlineKeyboardButton.WithUrl("❤️ TeknoLugu Dev", "https://t.me/TeknoLuguDev")
            },
            new[] {
                InlineKeyboardButton.WithUrl("📱 Redmi 5A (ID)", "https://t.me/Redmi5AID"),
                InlineKeyboardButton.WithUrl("🤖 Android APK", "https://t.me/ApkFreeID")
            },
            new[] {
                InlineKeyboardButton.WithUrl("😺 ZiziBot Play", "https://t.me/ZiziBotPlay"),
                InlineKeyboardButton.WithUrl("🏗 Akmal Projext", "https://t.me/AkmalProjext")
            },
            new[] {
                InlineKeyboardButton.WithUrl("🥛 Trakteer", "https://trakteer.id/azhe403/tip"),
                InlineKeyboardButton.WithUrl("🫲 Saweria", "https://saweria.co/azhe403")
            }
        });

        return await serviceFacade.TelegramService.SendMessageText(htmlMessage.ToString(), replyMarkup);
    }
}