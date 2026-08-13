using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using ZiziBot.Application.Common.Types;

namespace ZiziBot.Application.Services.Tasks;

public class StartupNotificationTask(
    IServiceScopeFactory serviceScope,
    ILogger<StartupNotificationTask> logger
) : IStartupTask
{
    public async Task ExecuteAsync()
    {
        using var service = serviceScope.CreateScope();
        var appSettingRepository = service.ServiceProvider.GetRequiredService<AppSettingRepository>();
        var botRepository = service.ServiceProvider.GetRequiredService<BotRepository>();

        var config = await botRepository.GetBotMain();
        var eventConfig = await appSettingRepository.GetRequiredConfigSectionAsync<EventLogConfig>();
        var engineConfig = await appSettingRepository.GetRequiredConfigSectionAsync<EngineConfig>();

        if (eventConfig.ChatId == 0)
        {
            logger.LogWarning("Startup notification skipped: EventLog ChatId is not configured");
            return;
        }

        var bot = new TelegramBotClient(config.Token);

        var message = HtmlMessage.Empty
            .Bold("Startup Notification").Br()
            .Bold("Date: ").Code(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss zz")).Br()
            .Bold("ExecutionStrategy: ").Code(engineConfig.ExecutionMode.ToString()).Br()
            .Bold("EngineMode: ").Code(engineConfig.TelegramEngineMode.ToString()).Br()
            .Text("#task #startup");

        try
        {
            await bot.SendMessage(eventConfig.ChatId, message.ToString(), parseMode: ParseMode.Html);
        }
        catch (ApiRequestException exception) when (exception.Message.Contains("chat not found", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(
                exception,
                "Startup notification skipped: Telegram chat {ChatId} was not found or is inaccessible",
                eventConfig.ChatId
            );
        }
    }
}
