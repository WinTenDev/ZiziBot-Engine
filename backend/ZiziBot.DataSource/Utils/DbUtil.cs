using MongoDB.Bson;
using MongoDB.Driver;

namespace ZiziBot.DataSource.Utils;

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

    public static ObjectId ToObjectId(this string input)
    {
        return ObjectId.Parse(input);
    }
}