using Kot.MongoDB.Migrations;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace ZiziBot.Kot.MongoMigrations.Seeders;

public class ApiKeySeedMigration(ILogger<ApiKeySeedMigration> logger, MongoDbContextBase mongoDbContext) : MongoMigration(DBVersion)
{
    private readonly ILogger<ApiKeySeedMigration> _logger = logger;
    private static DatabaseVersion DBVersion => new("232.26.3");

    public override async Task DownAsync(IMongoDatabase db, IClientSessionHandle session,
        CancellationToken cancellationToken)
    {
        await Task.Delay(1, cancellationToken);
    }

    public override async Task UpAsync(IMongoDatabase db, IClientSessionHandle session,
        CancellationToken cancellationToken)
    {
        mongoDbContext.ApiKey.Add(new ApiKeyEntity() {
            Name = ApiKeyVendor.Internal,
            Category = ApiKeyCategory.Internal,
            ApiKey = Guid.NewGuid().ToString(),
            Status = (int)EventStatus.InProgress
        });

        await mongoDbContext.SaveChangesAsync(cancellationToken);
    }
}