using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace ZiziBot.Presentation.Bots.Discord;

public class CommandHandlingService
{
	private readonly ILogger<CommandHandlingService> _logger;

	private readonly DiscordSocketClient _discord;
	private readonly CommandService _commands;
	private IServiceProvider _provider;

	public CommandHandlingService(IServiceProvider provider, ILogger<CommandHandlingService> logger, DiscordSocketClient discord, CommandService commands)
	{
		_logger = logger;
		_discord = discord;
		_commands = commands;
		_provider = provider;

		_discord.MessageReceived += MessageReceived;
	}

	public async Task InitializeAsync()
	{
		var asm = Assembly.GetExecutingAssembly();

		await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _provider);
		// Add additional initialization code here...
	}

	private async Task MessageReceived(SocketMessage rawMessage)
	{
		// Ignore system messages and messages from bots
		if (!(rawMessage is SocketUserMessage message)) return;
		if (message.Source != MessageSource.User) return;

		int argPos = 0;
		if (!message.HasMentionPrefix(_discord.CurrentUser, ref argPos)) return;

		var context = new SocketCommandContext(_discord, message);
		var result = await _commands.ExecuteAsync(context, argPos, _provider);

		if (result.Error.HasValue &&
		    result.Error.Value != CommandError.UnknownCommand)
			await context.Channel.SendMessageAsync(result.ToString());
	}
}

