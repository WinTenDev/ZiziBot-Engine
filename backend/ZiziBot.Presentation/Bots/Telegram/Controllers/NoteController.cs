using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Models;
using TgDeleteNoteRequest = ZiziBot.Application.Features.Handlers.Telegram.Note.DeleteNoteRequest;

namespace ZiziBot.Presentation.Bots.Telegram.Controllers;

// [BotName("Main")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class NotesController(
    MediatorService mediatorService
) : BotCommandController
{
    [Command("notes")]
    [Command("tags")]
    public async Task GetNotes(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new GetNoteBotRequestModel()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            ReplyMessage = true,
            DeleteAfter = TimeSpan.FromHours(1),
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }

    [Command("note")]
    [Command("renote")]
    public async Task CreateNote(CommandContext context)
    {
        var query = context.CommandParam.GetCommandParamAt<string>(0, separator: "\n");
        var rawButton = context.CommandParam.TrimStart(query);

        await mediatorService.EnqueueAsync(new CreateNoteBotRequest()
        {
            BotToken = context.BotToken,
            MinimumRole = RoleLevel.ChatAdminOrPrivate,
            Message = context.Message,
            ReplyMessage = true,
            Query = query,
            Content = context.Message.ReplyToMessage?.GetHtmlTextMarkup(),
            RawButton = rawButton,
            FileId = context.Message.ReplyToMessage?.GetFileId(),
            DataType = context.Message.ReplyToMessage != null ? (int)context.Message.ReplyToMessage.Type : -1,
            RefreshNote = context.Message.Text?.StartsWith("/renote"),
            DeleteAfter = TimeSpan.FromHours(1),
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }

    [Command("dnote")]
    public async Task DeleteNote(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new TgDeleteNoteRequest()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            Note = context.CommandParam,
            ReplyMessage = true,
            DeleteAfter = TimeSpan.FromMinutes(1),
            CleanupTargets = new[]
            {
                CleanupTarget.FromBot,
                CleanupTarget.FromSender
            }
        });
    }
}

