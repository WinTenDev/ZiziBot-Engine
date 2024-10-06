using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace ZiziBot.Application.Tasks;

public class StartupNotificationTask(AppSettingRepository appSettingRepository) : IStartupTask
{
    public bool SkipAwait { get; set; }

    public async Task ExecuteAsync()
    {
        var config = await appSettingRepository.GetBotMain();
        var eventConfig = await appSettingRepository.GetConfigSectionAsync<EventLogConfig>();

        if (eventConfig == null)
            return;

        var bot = new TelegramBotClient(config.Token);

        var message = HtmlMessage.Empty
            .Bold("Startup Notification").Br()
            .Bold("Date: ").Code(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss zz"));

        await bot.SendTextMessageAsync(eventConfig.ChatId, message.ToString(), parseMode: ParseMode.Html);
    }
}