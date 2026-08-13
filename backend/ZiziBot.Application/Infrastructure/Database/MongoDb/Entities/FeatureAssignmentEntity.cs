using MongoDB.EntityFrameworkCore;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("FeatureAssignment")]
public class FeatureAssignmentEntity : EntityBase
{
    public required string RolloutId { get; set; }
    public long ChatId { get; set; }
}
