using Kot.MongoDB.Migrations;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Telegram.Bot.Types.Enums;

namespace ZiziBot.Kot.MongoMigrations.Seeders;

public class BotCommandSeedMigration(ILogger<BotCommandSeedMigration> logger, MongoDbContextBase mongoDbContext) : MongoMigration(DBVersion)
{
    private readonly ILogger<BotCommandSeedMigration> _logger = logger;
    private static DatabaseVersion DBVersion => new("233.30.1");

    public override async Task DownAsync(IMongoDatabase db, IClientSessionHandle session, CancellationToken cancellationToken)
    {
        await Task.Delay(1, cancellationToken);
    }

    public override async Task UpAsync(IMongoDatabase db, IClientSessionHandle session, CancellationToken cancellationToken)
    {
        mongoDbContext.BotCommand.AddRange(new[]
        {
            new BotCommandEntity()
            {
                Command = "/ping",
                Description = "Mengecek kesehatan bot",
                Scope = BotCommandScopeType.Default,
                Status = (int)EventStatus.Complete
            },
            new BotCommandEntity()
            {
                Command = "/help",
                Description = "Menampilkan daftar perintah",
                Scope = BotCommandScopeType.Default,
                Status = (int)EventStatus.Complete
            },
            new BotCommandEntity()
            {
                Command = "/start",
                Description = "Memulai menggunakan bot",
                Scope = BotCommandScopeType.Default,
                Status = (int)EventStatus.Complete
            },
        });

        await mongoDbContext.SaveChangesAsync(cancellationToken);
    }
}