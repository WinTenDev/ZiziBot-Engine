using MongoDB.EntityFrameworkCore;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("PendekinMap")]
public class PendekinMapEntity : EntityBase
{
    public string OriginalUrl { get; set; }
    public string ShortPath { get; set; }
}
