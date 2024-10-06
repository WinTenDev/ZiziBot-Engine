using Kot.MongoDB.Migrations;
using MongoDB.Driver;

namespace ZiziBot.Kot.MongoMigrations.Seeders;

public class ChatRestrictionSeedMigration(MongoDbContextBase mongoDbContext) : MongoMigration(DBVersion)
{
    private static DatabaseVersion DBVersion => new("238.7.1");

    public override async Task DownAsync(IMongoDatabase db, IClientSessionHandle session, CancellationToken cancellationToken)
    {
    }

    public override async Task UpAsync(IMongoDatabase db, IClientSessionHandle session, CancellationToken cancellationToken)
    {
        mongoDbContext.ChatRestriction.AddRange(new List<ChatRestrictionEntity>()
        {
            new()
            {
                UserId = 123,
                ChatId = -1001404591750,
                Status = (int)EventStatus.Complete,
                TransactionId = Guid.NewGuid().ToString()
            },
            new()
            {
                UserId = 123,
                ChatId = -1001835114370,
                Status = (int)EventStatus.Complete,
                TransactionId = Guid.NewGuid().ToString()
            }
        });

        await mongoDbContext.SaveChangesAsync(cancellationToken);
    }
}