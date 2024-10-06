using Microsoft.EntityFrameworkCore;
using ZiziBot.DataSource.Sqlite.Entities;

namespace ZiziBot.DataSource.Sqlite;

public class SqliteCachingDbContext(string dbPath) : DbContext
{
    public DbSet<SqliteCacheEntity> SqliteCache { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        optionsBuilder.EnableSensitiveDataLogging();
    }
}