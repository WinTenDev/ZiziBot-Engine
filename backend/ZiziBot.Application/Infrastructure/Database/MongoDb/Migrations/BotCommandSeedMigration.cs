using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Interfaces;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Migrations;

public class BotCommandSeedMigration(ILogger<BotCommandSeedMigration> logger, MongoDbContext mongoDbContext) : IPostMigration
{
    readonly ILogger<BotCommandSeedMigration> _logger = logger;

    public async Task DownAsync()
    {
        await Task.Delay(1);
    }

    public async Task UpAsync()
    {
        mongoDbContext.BotCommand.AddRange(new List<BotCommandEntity>() {
            new() {
                Command = "/ping",
                Description = "Mengecek kesehatan bot",
                Scope = BotCommandScopeType.Default,
                Status = EventStatus.Complete
            },
            new() {
                Command = "/help",
                Description = "Menampilkan daftar perintah",
                Scope = BotCommandScopeType.Default,
                Status = EventStatus.Complete
            },
            new() {
                Command = "/start",
                Description = "Memulai menggunakan bot",
                Scope = BotCommandScopeType.Default,
                Status = EventStatus.Complete
            }
        });

        await mongoDbContext.SaveChangesAsync();
    }
}
