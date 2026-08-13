using MongoDB.EntityFrameworkCore;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("ApiKey")]
public class ApiKeyEntity : EntityBase
{
    public ApiKeyVendor Name { get; set; }
    public ApiKeyCategory Category { get; set; }
    public string ApiKey { get; set; }
    public DateTime? LastUsedDate { get; set; }
    public int? Usage { get; set; }
    public int? Remaining { get; set; }
    public int? Limit { get; set; }
    public string? LimitUnit { get; set; }
    public DateTime? ResetUsageDate { get; set; }
    public string? Note { get; set; }
    public string? Owner { get; set; }
}
