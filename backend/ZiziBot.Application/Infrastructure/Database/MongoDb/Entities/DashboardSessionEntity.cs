using MongoDB.EntityFrameworkCore;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("DashboardSession")]
public class DashboardSessionEntity : EntityBase
{
    public long TelegramUserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
    public string? PhotoUrl { get; set; }
    public long AuthDate { get; set; }
    public string? Hash { get; set; }
    public string? SessionId { get; set; }
    public required string BearerToken { get; set; }
    public DateTime? ExpireDate { get; set; }
}
