using Microsoft.EntityFrameworkCore;
using ZiziBot.Application.Infrastructure.Database.Sqlite.Entities;

namespace ZiziBot.Application.Infrastructure.Database.Sqlite;

public class SqliteCachingDbContext(string dbPath) : DbContext
{
    public DbSet<SqliteCacheEntity> SqliteCache { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        optionsBuilder.EnableSensitiveDataLogging();
    }
}
