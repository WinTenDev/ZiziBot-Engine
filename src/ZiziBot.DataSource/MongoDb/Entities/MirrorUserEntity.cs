using System.ComponentModel.DataAnnotations.Schema;

namespace ZiziBot.DataSource.MongoDb.Entities;

[Table("MirrorUser")]
public class MirrorUserEntity : EntityBase
{
    public long UserId { get; set; }
    public DateTime ExpireDate { get; set; }
}