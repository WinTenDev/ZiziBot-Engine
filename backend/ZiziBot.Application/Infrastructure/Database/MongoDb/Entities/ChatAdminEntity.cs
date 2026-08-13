using MongoDB.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("ChatAdmin")]
public class ChatAdminEntity : EntityBase
{
    public long UserId { get; set; }
    public long ChatId { get; set; }
    public ChatMemberStatus Role { get; set; }
}
