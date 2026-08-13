using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Interfaces;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb;

public class MigrationRunner(
    IServiceScopeFactory serviceProvider,
    ILogger<MigrationRunner> logger,
    MongoDbContext mongoDbContext
)
{
    private readonly string _transactionId = Guid.NewGuid().ToString();

    public async Task ApplyMigrationAsync()
    {
        logger.LogInformation("Starting Mongodb Migration");
        await using var scope = serviceProvider.CreateAsyncScope();
        var serviceScope = scope.ServiceProvider;

        var preMigrations = serviceScope.GetServices<IPreMigration>().ToList<IMigrationBase>();
        logger.LogDebug("Applying Pre-Migration. Total: {Count} Migrations", preMigrations.Count);
        await ApplyMigrationInternal(preMigrations);

        var migrations = serviceScope.GetServices<IMigration>().ToList<IMigrationBase>();
        logger.LogDebug("Applying Migration. Total: {Count} Migrations", migrations.Count);
        await ApplyMigrationInternal(migrations);

        var postMigrations = serviceScope.GetServices<IPostMigration>().ToList<IMigrationBase>();
        logger.LogDebug("Applying Post-Migration. Total: {Count} Migrations", postMigrations.Count);
        await ApplyMigrationInternal(postMigrations);

        logger.LogInformation("Mongodb Migration Completed");
    }

    private async Task ApplyMigrationInternal(List<IMigrationBase> migrations)
    {
        foreach (var migration in migrations)
        {
            var migrationName = migration.GetType().Name;

            var migrationType = migration switch {
                IPreMigration => "PreMigration",
                IMigration => "Migration",
                IPostMigration => "PostMigration",
                _ => throw new ArgumentOutOfRangeException()
            };

            var applied = await mongoDbContext.DbMigration
                .Where(x => x.MigrationName == migrationName)
                .FirstOrDefaultAsync();

            if (applied == null || migration is not IMigration)
            {
                var sw = Stopwatch.StartNew();
                logger.LogDebug("Applying Migration {Name}", migrationName);

                await migration.UpAsync();
                sw.Stop();

                mongoDbContext.DbMigration.Add(new DbMigrationEntity {
                    MigrationTypeName = migrationType,
                    MigrationName = migrationName,
                    AppliedAt = DateTime.UtcNow,
                    Elapsed = sw.Elapsed,
                    Status = EventStatus.Complete,
                    TransactionId = _transactionId
                });

                logger.LogDebug("Migration {Name} applied in {Elapsed}", migrationName, sw.Elapsed);
            }
            else
            {
                logger.LogDebug("Migration {Name} already applied", migrationName);
            }
        }

        await mongoDbContext.SaveChangesAsync();
    }

    public async Task RollbackMigrationAsync()
    {
        await Task.Delay(0);

        // TODO. rollback migration
    }
}
