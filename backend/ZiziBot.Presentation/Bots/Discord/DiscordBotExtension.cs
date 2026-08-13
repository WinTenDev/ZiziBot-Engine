using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ZiziBot.Presentation.Bots.Discord;

public static class DiscordBotExtension
{
    public static IServiceCollection AddDiscordBot(this IServiceCollection services)
    {
        services.AddScoped<DiscordSocketClient>();
        services.AddScoped<CommandHandlingService>();
        services.AddScoped<CommandService>();

        return services;
    }

    public static async Task<IApplicationBuilder> UseDiscordBot(this IApplicationBuilder app)
    {
        var serviceProvider = app.ApplicationServices.CreateScope().ServiceProvider;

        var client = new DiscordSocketClient();
        var config = serviceProvider.GetRequiredService<IConfiguration>();

        // services.GetRequiredService<LogService>();
        await serviceProvider.GetRequiredService<CommandHandlingService>().InitializeAsync();

        await client.LoginAsync(TokenType.Bot, "OTU0MzU0Mjk2ODI3MzAxOTQ5.GfMqwD.jTxsTz855tGtSlI7qIw_qRHGL6lV9fIWzadEnU");
        await client.StartAsync();

        await Task.Delay(-1);

        return app;
    }
}

