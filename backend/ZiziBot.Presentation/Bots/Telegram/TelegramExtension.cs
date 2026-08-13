using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using ZiziBot.Application.Infrastructure.Config;
using ZiziBot.Application.Common.Constants;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;
using ZiziBot.Application.Infrastructure.Database.Service;
using ZiziBot.TelegramBot.Framework.Extensions;
using ZiziBot.TelegramBot.Framework.Models.Configs;
using ZiziBot.TelegramBot.Framework.Models.Enums;

namespace ZiziBot.Presentation.Bots.Telegram;

public static class TelegramExtension
{
    public static IServiceCollection ConfigureTelegramBot(this IServiceCollection services)
    {
        services.AddZiziBotTelegramBot();

        return services;
    }

    public static async Task<IApplicationBuilder> RunTelegramBot(this IApplicationBuilder app)
    {
        await using var scope = app.ApplicationServices.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        var dataFacade = provider.GetRequiredService<DataFacade>();
        var engineOptions = provider.GetRequiredService<IOptions<EngineConfig>>().Value;
        var hostingEnvironment = provider.GetRequiredService<IHostEnvironment>();
        var botEngineConfig = provider.GetRequiredService<BotEngineConfig>();
        var listBotOptions = provider.GetRequiredService<List<BotTokenConfig>>();

        var listBotData = await dataFacade.Bot.ListBots();
        var botConfigs = listBotData.Select(x => new BotTokenConfig()
        {
            Name = x.Name,
            Token = x.Token
        }).ToList();

        listBotOptions.Clear();
        listBotOptions.AddRange(botConfigs);

        botEngineConfig.EngineMode = engineOptions.TelegramEngineMode;
        botEngineConfig.WebhookUrl = EnvUtil.GetEnv(Env.TELEGRAM_WEBHOOK_URL);
        botEngineConfig.ExecutionMode = engineOptions.ExecutionMode;
        botEngineConfig.Bot = listBotOptions;
        botEngineConfig.ActualEngineMode = botEngineConfig.EngineMode switch
        {
            BotEngineMode.Webhook => BotEngineMode.Webhook,
            BotEngineMode.Polling => BotEngineMode.Polling,
            _ => hostingEnvironment.IsDevelopment() ? BotEngineMode.Polling : BotEngineMode.Webhook
        };

        if (listBotOptions.Count == 0)
        {
            dataFacade.MongoDb.BotSettings.Add(new BotSettingsEntity()
            {
                Name = "Main",
                Token = "BOT_TOKEN_HERE",
                Status = EventStatus.InProgress
            });

            await dataFacade.MongoDb.SaveChangesAsync();

            throw new ApplicationException("No bot data found. Please ensure config for 'BotSettings'");
        }

        var validBot = listBotOptions
            .Where(x => x.Name != "BOT_NAME")
            .ToList();

        if (validBot.Count == 0)
        {
            throw new ApplicationException("Please add your Bot to 'BotSettings'");
        }

        await app.UseZiziBotTelegramBot();

        var bot = new TelegramBotClient(validBot.First(x => x.Name == "Main").Token);
        var me = await bot.GetMe();
        ValueConst.UniqueKey = me.GetFullName().Replace(" ", "_").ToLower() ?? string.Empty;

        return app;
    }
}
