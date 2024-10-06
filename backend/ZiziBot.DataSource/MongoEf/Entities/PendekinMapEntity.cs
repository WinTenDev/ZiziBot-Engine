namespace ZiziBot.DataSource.MongoEf.Entities;

[Table("PendekinMap")]
public class PendekinMapEntity : EntityBase
{
    public string OriginalUrl { get; set; }
    public string ShortPath { get; set; }
}