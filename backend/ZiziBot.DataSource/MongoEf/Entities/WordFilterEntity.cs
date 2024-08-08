namespace ZiziBot.DataSource.MongoEf.Entities;

[Table("WordFilter")]
public class WordFilterEntity : EntityBase
{
    public long ChatId { get; set; }
    public long UserId { get; set; }
    public string Word { get; set; }
    public bool IsGlobal { get; set; }
    public WordFilterAction[]? Action { get; set; }
}