using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ZiziBot.Utils;

public static class TelegramUtil
{
    public static string GetFullName(this User? user)
    {
        var fullName = (user.FirstName + " " + user.LastName).Trim();
        return fullName;
    }

    public static string GetFullMention(this User user)
    {
        var fullName = (user.FirstName + " " + user.LastName).Trim();
        var mention = "<a href=\"tg://user?id=" + user.Id + "\">" + fullName + "</a>";
        return mention;
    }

    public static string? GetFileId(this Message message)
    {
        var fileId = message.Type switch
        {
            MessageType.Photo => message.Photo?.LastOrDefault()?.FileId,
            MessageType.Audio => message.Audio?.FileId,
            MessageType.Video => message.Video?.FileId,
            MessageType.Voice => message.Voice?.FileId,
            MessageType.Document => message.Document?.FileId,
            MessageType.Sticker => message.Sticker?.FileId,
            MessageType.VideoNote => message.VideoNote?.FileId,
            _ => null
        };

        return fileId;
    }

    public static T GetInlineQueryAt<T>(this string query, int index)
    {
        dynamic value = query.Split(" ").ElementAtOrDefault(index);

        return Convert.ChangeType(value, typeof(T));
    }

    public static DateTime GetMessageDate(this Update update)
    {
        var date = update.Type switch
        {
            UpdateType.EditedMessage => update.EditedMessage?.EditDate.GetValueOrDefault(),
            UpdateType.EditedChannelPost => update.EditedChannelPost?.EditDate.GetValueOrDefault(),
            UpdateType.Message => update.Message?.Date,
            UpdateType.MyChatMember => update.MyChatMember?.Date,
            UpdateType.CallbackQuery => DateTime.UtcNow,
            UpdateType.ChannelPost => update.ChannelPost?.Date,
            UpdateType.ChatMember => update.ChatMember?.Date,
            UpdateType.ChatJoinRequest => update.ChatJoinRequest?.Date,
            _ => DateTime.UtcNow
        };

        return date ?? default;
    }

    public static DateTime GetMessageEditDate(this Update update)
    {
        var date = update.Type switch
        {
            UpdateType.EditedMessage => update.EditedMessage?.EditDate,
            UpdateType.EditedChannelPost => update.EditedChannelPost?.EditDate,
            _ => DateTime.UtcNow
        };

        return date ?? default;
    }
}