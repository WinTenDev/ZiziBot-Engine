using MongoDB.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("BotCommand")]
public class BotCommandEntity : EntityBase
{
    public string Command { get; set; }
    public string Description { get; set; }
    public BotCommandScopeType Scope { get; set; }
}
