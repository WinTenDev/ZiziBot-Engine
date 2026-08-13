using MongoDB.EntityFrameworkCore;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("FeatureRolloutCategory")]
public class FeatureRolloutCategoryEntity : EntityBase
{
    public required string Name { get; set; }
}
