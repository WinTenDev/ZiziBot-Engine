using MoreLinq.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ZiziBot.Application.Common.Utils;

public static class TelegramUtil
{
    public static string GetFullName(this User? user)
    {
        if (user is null) return string.Empty;

        var fullName = (user.FirstName + " " + user.LastName).Trim();
        return fullName;
    }

    public static string GetFullMention(this User user)
    {
        var fullName = (user.FirstName + " " + user.LastName).Trim();
        var mention = "<a href=\"tg://user?id=" + user.Id + "\">" + fullName + "</a>";
        return mention;
    }

    public static string? GetMessageLink(this Message? message)
    {
        if (message is null)
            return null;

        return message.Chat.Username is not null
            ? "https://t.me/" + message.Chat.Username + "/" + message.MessageId
            : "https://t.me/c/" + message.Chat.Id + "/" + message.MessageId;
    }

    public static string? GetFileId(this Message? message)
    {
        if (message is null) return default;

        var fileId = message.Type switch {
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

    public static string? GetFileUniqueId(this Message? message)
    {
        if (message is null) return default;

        var fileId = message.Type switch {
            MessageType.Photo => message.Photo?.LastOrDefault()?.FileUniqueId,
            MessageType.Audio => message.Audio?.FileUniqueId,
            MessageType.Video => message.Video?.FileUniqueId,
            MessageType.Voice => message.Voice?.FileUniqueId,
            MessageType.Document => message.Document?.FileUniqueId,
            MessageType.Sticker => message.Sticker?.FileUniqueId,
            MessageType.VideoNote => message.VideoNote?.FileUniqueId,
            _ => null
        };

        return fileId;
    }

    public static string? GetFileName(this Message message)
    {
        var fileName = message.Type switch {
            MessageType.Photo => message.Photo?.LastOrDefault()?.FileUniqueId + ".jpg",
            MessageType.Audio => message.Audio?.FileName,
            MessageType.Video => message.Video?.FileName,
            MessageType.Voice => message.Voice?.FileUniqueId + message.Voice?.MimeType?.Split("/").LastOrDefault(),
            MessageType.Document => message.Document?.FileName,
            _ => null
        };

        return fileName;
    }

    public static T GetInlineQueryAt<T>(this string query, int index)
    {
        dynamic value = query.Split(" ").ElementAtOrDefault(index) ?? string.Empty;

        return (T)Convert.ChangeType(value, typeof(T));
    }

    public static DateTime GetMessageDate(this Update update)
    {
        var date = update.Type switch {
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
        var date = update.Type switch {
            UpdateType.EditedMessage => update.EditedMessage?.EditDate,
            UpdateType.EditedChannelPost => update.EditedChannelPost?.EditDate,
            _ => DateTime.UtcNow
        };

        return date ?? default;
    }

    public static string GetHtmlTextMarkup(this Message message)
    {
        var htmlText = message.Text ?? message.Caption;
        var entities = message.Entities ?? message.CaptionEntities;
        var entityValues = message.EntityValues ?? message.CaptionEntityValues;

        if (htmlText == null) return string.Empty;
        if (entities is null || entityValues is null) return htmlText;

        entities.ForEach((entity, idx) => {
            var oldValue = entityValues.ElementAt(idx);
            var newValue = entity.Type switch {
                MessageEntityType.Bold => "<b>" + oldValue + "</b>",
                MessageEntityType.Code => "<code>" + oldValue + "</code>",
                MessageEntityType.CustomEmoji => "<tg-emoji emoji-id=\"" + entity.CustomEmojiId + "\">" + oldValue +
                                                 "</tg-emoji>",
                MessageEntityType.Italic => "<i>" + oldValue + "</i>",
                MessageEntityType.Pre => "<pre>" + oldValue + "</pre>",
                MessageEntityType.Strikethrough => "<s>" + oldValue + "</s>",
                MessageEntityType.Spoiler => "<tg-spoiler>" + oldValue + "</tg-spoiler>",
                MessageEntityType.TextLink => "<a href=\"" + entity.Url + "\">" + oldValue + "</a>",
                MessageEntityType.TextMention => "<a href=\"tg://user?id=" + entity.User?.Id + "\">" + oldValue +
                                                 "</a>",
                MessageEntityType.Underline => "<u>" + oldValue + "</u>",
                _ => oldValue
            };

            htmlText = htmlText.Replace(oldValue, newValue);
        });

        return htmlText;
    }

    public static string GetRawReplyMarkup(this Message? message)
    {
        var replyMarkup = message?.ReplyToMessage?.ReplyMarkup ?? message?.ReplyMarkup;

        if (replyMarkup is null) return string.Empty;

        var rawBtn = replyMarkup.InlineKeyboard.SelectMany(row =>
                row.Where(x => x.Url.IsNotNullOrEmpty()).Select(col => $"{col.Text}|{col.Url}"))
            .Aggregate((a, b) => a + "\n" + b);
        return rawBtn;
    }
}