using Telegram.Bot.Types.Enums;
using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.Presentation.Bots.Telegram.Controllers;

// [BotName("Main")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class ChatController(
    MediatorService mediatorServiceService
) : BotCommandController
{
    [Command("fch")]
    public async Task AddForwardChannelSource(CommandContext context)
    {
        await mediatorServiceService.EnqueueAsync(new ConnectChannelPostRequest()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            ReplyMessage = true,
            ChannelId = context.CommandParam.GetCommandParamAt<long>(0),
            MinimumRole = RoleLevel.ChatAdminOrPrivate,
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }

    [Command("pin")]
    public async Task PinMessage(CommandContext context)
    {
        await mediatorServiceService.EnqueueAsync(new PinMessageRequest()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            ReplyMessage = true,
            MinimumRole = RoleLevel.ChatAdminOrPrivate,
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }

    [UpdateCommand(UpdateType.ChatJoinRequest)]
    public async Task JoinRequest(CommandContext context)
    {
        await mediatorServiceService.EnqueueAsync(new ChatJoinBotRequest()
        {
            BotToken = context.BotToken,
            Update = context.Update,
            DeleteAfter = TimeSpan.FromMinutes(10),
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot
            }
        });
    }

    [TypedCommand(MessageType.NewChatMembers)]
    public async Task NewChatMembers(CommandContext context)
    {
        await mediatorServiceService.EnqueueAsync(new NewChatMembersBotRequest()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            NewUser = context.Message.NewChatMembers!,
            DeleteAfter = TimeSpan.FromMinutes(10),
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot
            }
        });
    }

    [UpdateCommand(UpdateType.ChannelPost)]
    [UpdateCommand(UpdateType.EditedChannelPost)]
    public async Task ChannelPost(CommandContext context)
    {
        await mediatorServiceService.EnqueueAsync(new ForwardChannelPostRequest()
        {
            BotToken = context.BotToken,
            Update = context.Update
        });
    }
}

