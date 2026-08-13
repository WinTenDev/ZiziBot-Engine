using MongoDB.EntityFrameworkCore;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("__migration")]
public class DbMigrationEntity : EntityBase
{
    public required string MigrationTypeName { get; set; }
    public required string MigrationName { get; set; }
    public DateTime AppliedAt { get; set; }
    public TimeSpan Elapsed { get; set; }
}
