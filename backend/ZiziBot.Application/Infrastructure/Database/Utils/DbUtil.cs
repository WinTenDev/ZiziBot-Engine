using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ZiziBot.Application.Infrastructure.Database.Utils;

public static class DbUtil
{
    public static MongoUrl ToMongoUrl(this string mongodbConnectionString)
    {
        var url = new MongoUrl(mongodbConnectionString);
        return url;
    }

    public static bool IsObjectId(this string? input)
    {
        return ObjectId.TryParse(input, out _);
    }

    public static ObjectId ToObjectId(this string? input)
    {
        if (input == null) return ObjectId.Empty;

        return ObjectId.Parse(input);
    }

    public static void ApplyDefaultValues(this ModelBuilder modelBuilder, Type propertyType, object defaultValue)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == propertyType);

            foreach (var property in properties)
            {
                var entity = modelBuilder.Entity(entityType.Name).Property(property.Name);
                entity.HasDefaultValue(defaultValue);
            }
        }
    }
}
