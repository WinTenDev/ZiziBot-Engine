using Allowed.Telegram.Bot.Attributes;
using Allowed.Telegram.Bot.Controllers;
using Allowed.Telegram.Bot.Models;
using Telegram.Bot.Types.Enums;

namespace ZiziBot.Allowed.TelegramBot.Controllers;

[BotName("Main")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class ChatController(MediatorService mediatorServiceService) : CommandController
{
    [Command("fch")]
    public async Task AddForwardChannelSource(MessageData data)
    {
        await mediatorServiceService.EnqueueAsync(new ConnectChannelPostRequest()
        {
            BotToken = data.Options.Token,
            Message = data.Message,
            ReplyMessage = true,
            ChannelId = data.Params.GetCommandParamAt<long>(0),
            MinimumRole = RoleLevel.ChatAdminOrPrivate,
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }

    [Command("pin")]
    public async Task PinMessage(MessageData data)
    {
        await mediatorServiceService.EnqueueAsync(new PinMessageRequest()
        {
            BotToken = data.Options.Token,
            Message = data.Message,
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
    public async Task JoinRequest(UpdateData data)
    {
        await mediatorServiceService.EnqueueAsync(new ChatJoinBotRequest()
        {
            BotToken = data.Options.Token,
            Update = data.Update,
            DeleteAfter = TimeSpan.FromMinutes(10),
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot
            }
        });
    }

    [TypedCommand(MessageType.ChatMembersAdded)]
    public async Task NewChatMembers(MessageData data)
    {
        await mediatorServiceService.EnqueueAsync(new NewChatMembersBotRequest()
        {
            BotToken = data.Options.Token,
            Message = data.Message,
            NewUser = data.Message.NewChatMembers!,
            DeleteAfter = TimeSpan.FromMinutes(10),
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot
            }
        });
    }

    [UpdateCommand(UpdateType.ChannelPost)]
    [UpdateCommand(UpdateType.EditedChannelPost)]
    public async Task ChannelPost(UpdateData data)
    {
        await mediatorServiceService.EnqueueAsync(new ForwardChannelPostRequest()
        {
            BotToken = data.Options.Token,
            Update = data.Update
        });
    }
}