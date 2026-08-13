using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Interfaces;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Migrations;

public class ApiKeySeedMigration(ILogger<ApiKeySeedMigration> logger, MongoDbContext mongoDbContext) : IMigration
{
    private readonly ILogger<ApiKeySeedMigration> _logger = logger;

    public async Task DownAsync()
    { }

    public async Task UpAsync()
    {
        var apiKey = "SOME_SECRET_KEY";

        if (await mongoDbContext.ApiKey.AnyAsync(x => x.ApiKey == apiKey))
            return;

        mongoDbContext.ApiKey.Add(new() {
            Name = ApiKeyVendor.Internal,
            Category = ApiKeyCategory.Internal,
            ApiKey = apiKey,
            Status = EventStatus.InProgress
        });

        await mongoDbContext.SaveChangesAsync();
    }
}
