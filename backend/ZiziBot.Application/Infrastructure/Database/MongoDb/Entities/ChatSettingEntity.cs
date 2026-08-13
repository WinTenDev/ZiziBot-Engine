using MongoDB.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("ChatSetting")]
public class ChatSettingEntity : EntityBase
{
    public long ChatId { get; set; }
    public string? ChatTitle { get; set; }
    public ChatType ChatType { get; set; }
    public string? ChatTypeName { get; set; }

    public bool IsAdmin { get; set; }

    public bool EnableBot { get; set; }
    public bool EnableAfkStatus { get; set; }
    public bool EnableCheckProfilePhoto { get; set; }
    public bool EnableFedCasBan { get; set; }
    public bool EnableFedEssBan { get; set; }
    public bool EnableFedSpamWatch { get; set; }
    public bool EnableChannelSubscription { get; set; }
    public bool EnableFindNotes { get; set; }
    public bool EnablePrivacyMode { get; set; }
    public bool EnableReplyNotification { get; set; }
    public bool EnableRestriction { get; set; }
    public bool EnableScanMedia { get; set; }
    public bool EnableScanLink { get; set; }
    public bool EnableScanText { get; set; }
    public bool EnableSpellCheck { get; set; }
    public bool EnableWarnUsername { get; set; }
    public bool EnableWelcomeMessage { get; set; }
    public bool EnableZiziMata { get; set; }

    public long EventLogChatId { get; set; }

    public string? LanguageCode { get; set; }
}
