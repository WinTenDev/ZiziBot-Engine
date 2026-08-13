using Telegram.Bot.Types.Enums;
using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.Presentation.Bots.Telegram.Controllers;

// [BotName("Main")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class DebugController(
    MediatorService mediatorService
) : BotCommandController
{
    [Command("about")]
    [Command("start")]
    public async Task GetAbout(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new GetAboutBotRequest()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            ReplyMessage = true,
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }

    [Command("id")]
    public async Task GetId(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new GetIdBotRequestModel()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            ReplyMessage = true,
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }

    [Command("si")]
    public async Task GetSystemInfo(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new GetSystemInfoRequest()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            MinimumRole = RoleLevel.Sudo,
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }

    [Command("fid")]
    public async Task GetFileId(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new GetFileIdBotRequest()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            ReplyMessage = true,
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }

    [Command("dbg")]
    [Command("yaml")]
    [Command("json")]
    public async Task GetDebug(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new GetDebugBotRequest()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            ReplyMessage = true,
            CleanupTarget = CleanupTarget.FromBot | CleanupTarget.FromSender,
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }

    [Command("promote")]
    [Command("demote")]
    public async Task PromoteMember(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new PromoteMemberBotRequest()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            ReplyMessage = true,
            Promote = context.Message.Text.IsCommand("/promote"),
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }

    [TypedCommand(MessageType.ForumTopicCreated)]
    [TypedCommand(MessageType.ForumTopicEdited)]
    public async Task OnTopicChange(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new ThreadUpdateBotRequest()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            ReplyMessage = true,
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }
}

