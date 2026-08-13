using MongoDB.EntityFrameworkCore;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("FeatureRollout")]
public class FeatureRolloutEntity : EntityBase
{
    public required string FeatureName { get; set; }
    public FeatureRolloutCategory RolloutCategory { get; set; }
}
