using MongoDB.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("ChatUser")]
public class ChatUserEntity : EntityBase
{
    public long ChatId { get; set; }
    public ChatType ChatType { get; set; }
    public int MemberCount { get; set; }
    public string? ChatUsername { get; set; }
    public string? ChatTitle { get; set; }
    public bool IsBotAdmin { get; set; }
}
