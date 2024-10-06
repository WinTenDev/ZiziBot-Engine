using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using ZiziBot.DataSource.MongoEf.Entities;

namespace ZiziBot.DataSource.MongoEf;

public class MongoEfContext : DbContext
{
    private readonly string _connectionString = EnvUtil.GetEnv(Env.MONGODB_CONNECTION_STRING, throwIsMissing: true);

    public DbSet<ChatAdminEntity> ChatAdmin { get; set; }
    public DbSet<WordFilterEntity> WordFilter { get; set; }

    public DbSet<WebhookHistoryEntity> WebhookHistory { get; set; }

    public DbSet<PendekinMapEntity> PendekinMap { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var conn = MongoUrl.Create(_connectionString);

        optionsBuilder.UseMongoDB(_connectionString, conn.DatabaseName);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        EnsureTimestamp();

        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new())
    {
        EnsureTimestamp();

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void EnsureTimestamp()
    {
        var entries = ChangeTracker.Entries<EntityBase>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);

        foreach (var entityEntry in entries)
            switch (entityEntry.State)
            {
                case EntityState.Added:
                    entityEntry.Entity.CreatedDate = DateTime.UtcNow;
                    entityEntry.Entity.UpdatedDate = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entityEntry.Entity.UpdatedDate = DateTime.UtcNow;
                    break;

                case EntityState.Deleted:
                    entityEntry.State = EntityState.Modified; // prevent hard delete
                    entityEntry.Entity.Status = EventStatus.Deleted; // mark as deleted
                    entityEntry.Entity.UpdatedDate = DateTime.UtcNow;
                    break;
            }
    }
}