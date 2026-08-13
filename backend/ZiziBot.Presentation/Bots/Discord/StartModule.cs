using Discord.Commands;

namespace ZiziBot.Presentation.Bots.Discord;

public class StartModule : ModuleBase<SocketCommandContext>
{
	[Command("ping")]
	[Alias("pong", "hello")]
	public Task PingAsync()
		=> ReplyAsync("pong!");
}

